/// this is an example of WAMP in javascript
/// support by VRMaker<www.vrmaker.com.cn>

const WampCommon = require('./WampCommon.js');

var ClassTest = {
    // description: 
    // callback: Void
    // arguments: 
    AddOne: function (callback) {
        var args = new Array();
        WampCommon.WampCall('ClassTest', 'AddOne', args, callback);
    },
    // description: 
    // callback: Void
    // arguments: num:Int32 
    Add: function (num, callback) {
        var args = new Array(num);
        WampCommon.WampCall('ClassTest', 'Add', args, callback);
    },
    // description: 
    // callback: Int32
    // arguments: 
    Get: function (callback) {
        var args = new Array();
        WampCommon.WampCall('ClassTest', 'Get', args, callback);
    },
    // description: 
    // callback arguments: value:String 
    AddValuedChanged: function (callback) {
        WampCommon.WampSubscribe('ClassTest', 'ValuedChanged', callback);
    },
    // description: 
    // callback arguments: value:String 
    DeleteValuedChanged: function (callback) {
        WampCommon.WampUnsubscribe('ClassTest', 'ValuedChanged', callback);
    },
};

export default ClassTest;
