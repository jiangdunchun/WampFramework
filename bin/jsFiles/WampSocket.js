/// this is the basic module of WAMP in javascript
/// support by Jiang Dunchun<jiangdunchun@outlook.com>

var _socket = null;
var _openCallbacks = [];
var _closeCallbacks = [];

// if the socket not connected, start connection
var WampStart = function (_wamp_socket, __socket_opened, __socket_closed, __socket_received) {
    if (_socket != null) {
        return ;
    }
    var wsImpl = window.WebSocket || window.MozWebSocket;
    var uri = 'ws://' + _wamp_socket.host + ':' + _wamp_socket.port  + "/" + _wamp_socket.router;
    var ws = new wsImpl(uri);
    
    _socket = ws;
    ws.onmessage = (evt) => {
        if(__socket_received){
            __socket_received(evt);
        }
        
    }

    ws.onopen = (ws) => {
        if(__socket_opened){
            __socket_opened(ws);
        }
        for(var i in _openCallbacks){
            _openCallbacks[i]();
        }
    }

    ws.onclose = () => {
        if(__socket_closed){
            __socket_closed();
        }
        for(var i in _closeCallbacks){
            _closeCallbacks[i].callbacks();
        }
    }
};

var WampSocket = {
    addSocketOpenEvent : function(event){
        _openCallbacks.push(event);
    },
    removeSocketOpenEvent : function(event){
        for(var i in _openCallbacks){
            if(_openCallbacks[i] == event){
                _openCallbacks.slice(i, 1);
            }
        }
    },
    addSocketCloseEvent : function(event){
        _closeCallbacks.push(event)
    },
    removeSocketCloseEvent : function(event){
        for(var i in _closeCallbacks){
            if(_closeCallbacks[i] == event){
                _closeCallbacks.slice(i, 1);
            }
        }
    },
    startConnect : function(_wamp_socket, __socket_opened, __socket_closed, __socket_received){
        WampStart(_wamp_socket, __socket_opened, __socket_closed, __socket_received);
    }
}

module.exports = WampSocket;



