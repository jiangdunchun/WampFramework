/// this is the basic module of WAMP in javascript
/// support by Dunchun Jiang<jiangdunchun@outlook.com>

const _wamp_socket = require('./WampConfig.js');

var WampProtocol = {
    CAL: 0,
    CAL_SUC: 8,
    CAL_FAL: 9,

    SBS: 10,
    SBS_BCK: 11,
    SBS_SUC: 18,
    SBS_FAL: 19,

    UNSBS: 20,
    UNSBS_SUC: 28,
    UNSBS_FAL: 29,

    REG: 30,
    REG_SUC: 38,
    REG_FAL: 39,

    ERRO = 101
}

// the socket connecting to wamp server
var _socket = null;

// record all events, insert a new one when call WampSubscribe, 
// delete the old one when receive subscribe false or unsubscribe success message from wamp server
// _events_pool[ID] = {
//     ENTITY : ,
//     NAME : ,
//     CALLBACK : ,
// };
var _events_pool = new Object();

// record all methods, insert a new one when call WampCall, 
// delete the old one when receive call back message from wamp server, 
// no matter this message is success or fail
// _methods_pool[ID] = {
//     ENTITY : ,
//     NAME : ,
//     CALLBACK : ,
//     ARGS:
// };
var _methods_pool = new Object();

// if the socket is connecting to wamp server
var __is_connected = function () {
    return _socket != null ? _socket.readyState === 1 : FAL;
};

// generate an unique id, the id needs to less than 65536 because it's format is ushort
var __generate_id = function (isCall) {
    var id = Math.floor(Math.random() * 65536);
    if (isCall) {
        if (_methods_pool[id] != null) {
            return __generate_id(isCall);
        }
        else {
            return id;
        }
    }
    else {
        if (_events_pool[id] != null) {
            return __generate_id(isCall);
        }
        else {
            return id;
        }
    }
};

// send message to wamp server
var __send_protocol = function (protocal, id, entity, name, args = null) {
    if (!__is_connected()) return;

    var send_msg = protocal + ',' + id + ',' + entity + ',' + name;
    var args_msg = "";
    for (j = 0; args != null && args[j] != null; j++) {
        args_msg += args[j] + ',';
    };
    if (args_msg != "") {
        args_msg = args_msg.substr(0, args_msg.length - 1);
    }
    _socket.send(send_msg + "," + args_msg);
};

// resolve the message from wamp server, return a data structure
var __resolve_msg = function (message) {
    var rec_array = message.data.split(",");
    var args = [];

    if (rec_array.length > 4) {
        for (i = 4; i < rec_array.length; i++) {
            if (rec_array[i] == "") break;
            args.push(rec_array[i]);
        };
    }
    var rec_msg = {
        PROTOCAL: rec_array[0],
        ID: rec_array[1],
        ENTITY: rec_array[2],
        NAME: rec_array[3],
        ARGS: args
    };

    return rec_msg;
};

// when wamp server sends message back
var __socket_received = function (message) {
    var rec_msg = __resolve_msg(message);

    switch (parseInt(rec_msg.PROTOCAL)) {
        case WampProtocol.CAL_SUC:
            _methods_pool[rec_msg.ID].CALLBACK(rec_msg.ARGS);
            delete _methods_pool[rec_msg.ID];
            break;
        case WampProtocol.CAL_FAL:
            delete _methods_pool[rec_msg.ID];
            break;
        case WampProtocol.SBS_SUC:
            break;
        case WampProtocol.SBS_FAL:
            delete _events_pool[rec_msg.ID];
            break;
        case WampProtocol.SBS_BCK:
            _events_pool[rec_msg.ID].CALLBACK(rec_msg.ARGS);
            break;
        case WampProtocol.UNSBS_SUC:
            delete _events_pool[rec_msg.ID];
            break;
        case WampProtocol.UNSBS_FAL:
            break;
    }
};

// when open the connection to wamp server
var __socket_opened = function (ws) {
};

// when close the connection to wamp server
var __socket_closed = function () {
};



var WampCommon = function (server) {
    this.WampStart(server);
}

WampCommon.prototype = {
    // start the connection to wamp server in the first time when this module is used
    // if the socket not connected, start connection
    WampStart: function () {
        if (_socket != null) return;

        var wsImpl = window.WebSocket || window.MozWebSocket;
        var uri = 'ws://' + _wamp_socket.host + ':' + _wamp_socket.port + '/';
        var ws = new wsImpl(uri);

        _socket = ws;

        ws.onmessage = (evt) => {
            __socket_received(evt);
        }

        ws.onopen = (ws) => {
            __socket_opened(ws);
        }

        ws.onclose = () => {
            __socket_closed();
        }
    },

    // call a wamp function
    WampCall: function (entity, name, args, callback) {
        var id = __generate_id(true);

        _methods_pool[id] = {
            ENTITY: entity,
            NAME: name,
            CALLBACK: callback,
            ARGS: args
        };

        __send_protocol(WampProtocol.CAL, id, entity, name, args);
    },

    // register a wamp event
    WampSubscribe: function (entity, name, callback) {
        var id = __generate_id(false);

        _events_pool[id] = {
            ENTITY: entity,
            NAME: name,
            CALLBACK: callback,
        };

        __send_protocol(WampProtocol.SBS, id, entity, name);
    },

    // cancel an event's register
    WampUnsubscribe: function (entity, name, callback) {
        var id = -1;
        for (var i in _events_pool) {
            if (_events_pool[i].ENTITY == entity && _events_pool[i].NAME == name && _events_pool[i].CALLBACK == callback) {
                id = i;
                break;
            }
        }
        if (id != -1) {
            __send_protocol(WampProtocol.UNSBS, id, entity, name);
        }
    },
};


export default WampCommon;




