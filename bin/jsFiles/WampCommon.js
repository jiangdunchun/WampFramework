/// this is the basic module of WAMP in javascript
/// support by Jiang Dunchun<jiangdunchun@outlook.com>

import _socketConfig from './WampConfig';
import Message_Format from './WampFormat';
import WampSocket from './WampSocket';

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

    ERRO : 101
}

var ArgsTypes = {
    NULL: 0,
    STRING: 1,
    BYTE: 2,
    BOOL: 3,
    USHORT : 4,
    SHORT : 5,
    INT: 6,
    FLOAT: 7,
    DOUBLE: 8,
    JSON : 9,

    STRINGS : 11,
    BYTES : 12,
    BOOLS : 13,
    USHORTS : 14,
    SHORTS : 15,
    INTS : 16,
    FLOATS  : 17,
    DOUBLES  : 18,
    JSONS  : 19,

    
}
// var reader = new FileReader();

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
    return _socket != null ? _socket.readyState === 1 : 0;
};

// generate an unique id, the id needs to less than 65536 because it's format is Uint16
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

    var send_msg = protocal + '|' + id + '|' + entity + '|' + name;
    var args_msg = "";
    for (var j = 0; args != null && args[j] != null; j++) {
        args_msg += args[j] + '|';
    };
    if (args_msg != "") {
        args_msg = args_msg.substr(0, args_msg.length - 1);
        send_msg = send_msg + "|" + args_msg; 
    }
    if(name != "SetMouseMove"){
        console.log(send_msg)
    }
    
    _socket.send(send_msg);
};

// resolve the message from wamp server, return a data structure
var __resolve_msg = function (message, callback) {
    var args = [];
    var rec_msg = {}
    
    if(typeof message.data == 'string' || message.data instanceof String){
        var rec_array = message.data.split("|");
        if (rec_array.length > 4) {
            for (var i = 4; i < rec_array.length; i++) {
                if (rec_array[i] == "") break;
                args.push(rec_array[i]);
            };
        }

        rec_msg = {
            PROTOCAL: rec_array[0],
            ID: rec_array[1],
            ENTITY: rec_array[2],
            NAME: rec_array[3],
            ARGS: args
        };

        callback(rec_msg);

    }else if(typeof message.data == 'blob' || message.data instanceof Blob){
        var reader = new FileReader();
        reader.readAsArrayBuffer(message.data);
        reader.onloadend = function(e){
            var Split_Tab = 0;
            var _PROTOCAL = new Uint8Array(reader.result.slice(Split_Tab, Split_Tab + 1));
            Split_Tab += 1;
            rec_msg.PROTOCAL = _PROTOCAL[0];

            var _ID = new Uint16Array(reader.result.slice(Split_Tab, Split_Tab + 2));
            Split_Tab += 2;
            rec_msg.ID = _ID[0];

            var _ENTITY_SIZE = new Uint8Array(reader.result.slice(Split_Tab, Split_Tab + 1));
            Split_Tab += 1;

            var _ENTITY_BUFFER = new Uint8Array(reader.result.slice(Split_Tab, Split_Tab + _ENTITY_SIZE[0]));
            Split_Tab += _ENTITY_SIZE[0];
            var _ENTITY = String.fromCharCode.apply(null, new Uint16Array(_ENTITY_BUFFER));
            rec_msg.ENTITY = _ENTITY.split(",")[0];
            rec_msg.NAME = _ENTITY.split(",")[1];

            var _ARGS_TYPR = new Uint8Array(reader.result.slice(Split_Tab, Split_Tab + 1));
            console.log(_ARGS_TYPR)
            Split_Tab += 1;
            var arrayBuffer = reader.result;
            rec_msg.ARGS = _format_args(new Array(), arrayBuffer, Split_Tab, _ARGS_TYPR[0]);

            reader = null;
            callback(rec_msg);
        }
        
    }
};


var _format_args = function(_ARGS, arrayBuffer, Split_Tab, Type){
    var ByteLength = arrayBuffer.byteLength;
    console.log(Type,ByteLength)
    switch(Type) {
        case ArgsTypes.NULL :
            _ARGS.push(null);
            break;
        case ArgsTypes.STRING :
            _ARGS.push(Message_Format.ToString(arrayBuffer, Split_Tab, 4));
            var _STR_LENGTH = new Uint32Array(arrayBuffer.slice(Split_Tab, Split_Tab + 4));
            Split_Tab += _STR_LENGTH[0];
            break;
        case ArgsTypes.BYTE :
            _ARGS.push(Message_Format.ToByte(arrayBuffer, Split_Tab, 1));
            Split_Tab += 1;
            break;
        case ArgsTypes.BOOL :
            _ARGS.push(Message_Format.ToBool(arrayBuffer, Split_Tab, 1));
            Split_Tab += 1;
            break;
        case ArgsTypes.USHORT  :
            _ARGS.push(Message_Format.ToUShort(arrayBuffer, Split_Tab, 2));
            Split_Tab += 2;
            break;
        case ArgsTypes.SHORT  :
            _ARGS.push(Message_Format.ToShort(arrayBuffer, Split_Tab, 2));
            Split_Tab += 2;
            break;
        case ArgsTypes.INT :
            _ARGS.push(Message_Format.ToInt(arrayBuffer, Split_Tab, 4));
            Split_Tab += 4;
            break;
        case ArgsTypes.FLOAT :
            _ARGS.push(Message_Format.ToFloat(arrayBuffer, Split_Tab, 4));
            Split_Tab += 4;
            break;
        case ArgsTypes.DOUBLE :
            _ARGS.push(Message_Format.ToDouble(arrayBuffer, Split_Tab, 8));
            Split_Tab += 8;
            break;
        case ArgsTypes.JSON :
            _ARGS.push(JSON.parse(Message_Format.ToString(arrayBuffer, Split_Tab, 4)));
            var _STR_LENGTH = new Uint32Array(arrayBuffer.slice(Split_Tab, Split_Tab + 4));
            Split_Tab += _STR_LENGTH[0];
            break;

        case ArgsTypes.STRINGS :
            var _ARRAY_LENGTH = new Uint32Array(arrayBuffer.slice(Split_Tab, Split_Tab + 4));
            Split_Tab += 4;
            var split_arrayBuffer = arrayBuffer.slice(Split_Tab,Split_Tab + _ARRAY_LENGTH[0]);
            _ARGS.push(Message_Format.ToStringArray(split_arrayBuffer, 0, 4, _ARRAY_LENGTH[0], 0, new Array()));
            Split_Tab += _ARRAY_LENGTH[0]*1;
            break;
        case ArgsTypes.BYTES :
            var _ARRAY_LENGTH = new Uint32Array(arrayBuffer.slice(Split_Tab, Split_Tab + 4));
            Split_Tab += 4;
            var split_arrayBuffer = arrayBuffer.slice(Split_Tab,Split_Tab + _ARRAY_LENGTH[0]);
            _ARGS.push(Message_Format.ToByteArray(split_arrayBuffer, 0, 1, _ARRAY_LENGTH[0], 0, new Array()));
            Split_Tab += _ARRAY_LENGTH[0]*1;
            break;
        case ArgsTypes.BOOLS :
            var _ARRAY_LENGTH = new Uint32Array(arrayBuffer.slice(Split_Tab, Split_Tab + 4));
            Split_Tab += 4;
            var split_arrayBuffer = arrayBuffer.slice(Split_Tab,Split_Tab + _ARRAY_LENGTH[0]);
            _ARGS.push(Message_Format.ToBoolArray(split_arrayBuffer, 0, 1, _ARRAY_LENGTH[0], 0, new Array()));
            Split_Tab += _ARRAY_LENGTH[0]*1;
            break;
        case ArgsTypes.USHORTS  :
            var _ARRAY_LENGTH = new Uint32Array(arrayBuffer.slice(Split_Tab, Split_Tab + 4));
            Split_Tab += 4;
            var split_arrayBuffer = arrayBuffer.slice(Split_Tab,Split_Tab + _ARRAY_LENGTH[0]);
            _ARGS.push(Message_Format.ToUShortArray(split_arrayBuffer, 0, 2, _ARRAY_LENGTH[0], 0, new Array()));
            Split_Tab += _ARRAY_LENGTH[0]*1;
            break;
        case ArgsTypes.SHORTS  :
            var _ARRAY_LENGTH = new Uint32Array(arrayBuffer.slice(Split_Tab, Split_Tab + 4));
            Split_Tab += 4;
            var split_arrayBuffer = arrayBuffer.slice(Split_Tab,Split_Tab + _ARRAY_LENGTH[0]);
            _ARGS.push(Message_Format.ToShortArray(split_arrayBuffer, 0, 2, _ARRAY_LENGTH[0], 0, new Array()));
            Split_Tab += _ARRAY_LENGTH[0]*1;
            break;
        case ArgsTypes.INTS :
            var _ARRAY_LENGTH = new Uint32Array(arrayBuffer.slice(Split_Tab, Split_Tab + 4));
            Split_Tab += 4;
            var split_arrayBuffer = arrayBuffer.slice(Split_Tab,Split_Tab + _ARRAY_LENGTH[0]);
            _ARGS.push(Message_Format.ToIntArray(split_arrayBuffer, 0, 4, _ARRAY_LENGTH[0], 0, new Array()));
            Split_Tab += _ARRAY_LENGTH[0]*1;
            break;
        case ArgsTypes.FLOATS :
            var _ARRAY_LENGTH = new Uint32Array(arrayBuffer.slice(Split_Tab, Split_Tab + 4));
            Split_Tab += 4;
            var split_arrayBuffer = arrayBuffer.slice(Split_Tab,Split_Tab + _ARRAY_LENGTH[0]);
            _ARGS.push(Message_Format.ToFloatArray(split_arrayBuffer, 0, 4, _ARRAY_LENGTH[0], 0, new Array()));
            Split_Tab += _ARRAY_LENGTH[0]*1;
            break;
        case ArgsTypes.DOUBLES :
            var _ARRAY_LENGTH = new Uint32Array(arrayBuffer.slice(Split_Tab, Split_Tab + 4));
            Split_Tab += 4;
            var split_arrayBuffer = arrayBuffer.slice(Split_Tab,Split_Tab + _ARRAY_LENGTH[0]);
            _ARGS.push(Message_Format.ToDoubleArray(split_arrayBuffer, 0, 8, _ARRAY_LENGTH[0], 0, new Array()));
            Split_Tab += _ARRAY_LENGTH[0]*1;
            break;
        case ArgsTypes.JSONS  :
            var _ARRAY_LENGTH = new Uint32Array(arrayBuffer.slice(Split_Tab, Split_Tab + 4));
            Split_Tab += 4;
            var split_arrayBuffer = arrayBuffer.slice(Split_Tab,Split_Tab + _ARRAY_LENGTH[0]);
            _ARGS.push(Message_Format.ToJSONArray(split_arrayBuffer, 0, 4, _ARRAY_LENGTH[0], 0, new Array()));
            Split_Tab += _ARRAY_LENGTH[0]*1;
            break;
    }
    if(Split_Tab < ByteLength){
        var _type = new Uint8Array(arrayBuffer.slice(Split_Tab, Split_Tab + 1));
        Split_Tab += 1;
        var new_Split = Split_Tab;
        return _format_args(_ARGS, arrayBuffer, new_Split, _type[0]);
    }else{
        return _ARGS;
    }
}

// when wamp server sends message back
var __socket_received = function (message) {
    __resolve_msg(message, function(rec_msg){
        switch (parseInt(rec_msg.PROTOCAL)) {
            case WampProtocol.CAL_SUC:
                if(_methods_pool[rec_msg.ID].CALLBACK && _methods_pool[rec_msg.ID].CALLBACK != null){
                    _methods_pool[rec_msg.ID].CALLBACK(rec_msg.ARGS);
                }
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
                console.log(_methods_pool)
                if(_events_pool[rec_msg.ID].CALLBACK && _events_pool[rec_msg.ID].CALLBACK != null){
                    _events_pool[rec_msg.ID].CALLBACK(rec_msg.ARGS);
                }
                break;
            case WampProtocol.UNSBS_SUC:
                delete _events_pool[rec_msg.ID];
                break;
            case WampProtocol.UNSBS_FAL:
                break;
        }
    });
    
};

// when the connection to wamp server was opened
var __socket_opened = function (ws) {
    _socket = ws.target;
};

// when the connection to wamp server was closed
var __socket_closed = function (ws) {

};

var WampCommon = {
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
            if (_events_pool[i].ENTITY == entity && _events_pool[i].NAME == name) {
                id = i;
                break;
            }
        }
        if (id != -1) {
            __send_protocol(WampProtocol.UNSBS, id, entity, name);
        }
    },
};
WampSocket.startConnect(_socketConfig, __socket_opened,__socket_closed, __socket_received )

module.exports = WampCommon;



