var utf16ToUtf8 = function (utf16Str) {
    var utf8Arr = [];
    var byteSize = 0;
    for (var i = 0; i < utf16Str.length; i++) {
        //获取字符Unicode码值
        var code = utf16Str.charCodeAt(i);

        //如果码值是1个字节的范围，则直接写入
        if (code >= 0x00 && code <= 0x7f) {
            byteSize += 1;
            utf8Arr.push(code);

            //如果码值是2个字节以上的范围，则按规则进行填充补码转换
        } else if (code >= 0x80 && code <= 0x7ff) {
            byteSize += 2;
            utf8Arr.push((192 | (31 & (code >> 6))));
            utf8Arr.push((128 | (63 & code)))
        } else if ((code >= 0x800 && code <= 0xd7ff)
            || (code >= 0xe000 && code <= 0xffff)) {
            byteSize += 3;
            utf8Arr.push((224 | (15 & (code >> 12))));
            utf8Arr.push((128 | (63 & (code >> 6))));
            utf8Arr.push((128 | (63 & code)))
        } else if(code >= 0x10000 && code <= 0x10ffff ){
            byteSize += 4;
            utf8Arr.push((240 | (7 & (code >> 18))));
            utf8Arr.push((128 | (63 & (code >> 12))));
            utf8Arr.push((128 | (63 & (code >> 6))));
            utf8Arr.push((128 | (63 & code)))
        }
    }

    return utf8Arr
}

var WampValueHelper = {
    ToString : function(arrayBuffer, start, size){
        var _tab = start;
        var _STR_LENGTH = new Uint32Array(arrayBuffer.slice(start, _tab + size));
        console.log(_STR_LENGTH[0])
        _tab += size;
        var CONTRNTBUFFER = new Uint8Array(arrayBuffer.slice(_tab, _tab + _STR_LENGTH[0]));

        var fileR = new FileReader();
        var bbb = new Uint8Array(CONTRNTBUFFER)
        fileR.readAsText(new Blob([bbb]), 'utf-8');
        fileR.onload = function () {
            console.info(fileR.result); //中文字符串
        };


        var _CONTENT = String.fromCharCode.apply("", new Uint16Array(CONTRNTBUFFER));
        console.log(_CONTENT)
        _CONTENT = decodeURIComponent(escape(_CONTENT));
        console.log(_CONTENT)
        start += _STR_LENGTH[0];
        return _CONTENT;
    },
    ToByte : function(arrayBuffer, start, size){
        var _tab = start;
        var CONTRNTBUFFER = new Uint8Array(arrayBuffer.slice(start, _tab + size));
        var _CONTENT = new Blob(CONTRNTBUFFER);
        
        return _CONTENT;
    },
    ToBool : function(arrayBuffer, start, size){
        var _tab = start;
        var CONTRNTBUFFER = new Uint8Array(arrayBuffer.slice(start, _tab + size));
        var _CONTENT = CONTRNTBUFFER[0];
        
        return _CONTENT;
    },
    ToShort : function(arrayBuffer, start, size){
        var _tab = start;
        var CONTRNTBUFFER = new int16Array(arrayBuffer.slice(start, _tab + size));
        var _CONTENT = CONTRNTBUFFER[0];
        
        return _CONTENT;
    },
    ToUShort : function(arrayBuffer, start, size){
        var _tab = start;
        var CONTRNTBUFFER = new Uint16Array(arrayBuffer.slice(start, _tab + size));
        var _CONTENT = CONTRNTBUFFER[0];
        
        return _CONTENT;
    },
    ToInt : function(arrayBuffer, start, size){
        var _tab = start;
        var CONTRNTBUFFER = new Int32Array(arrayBuffer.slice(start, _tab + size));
        var _CONTENT = CONTRNTBUFFER[0];
        
        return _CONTENT;
    },
    ToFloat : function(arrayBuffer, start, size){
        var _tab = start;
        var CONTRNTBUFFER = new Float32Array(arrayBuffer.slice(start, _tab + size));
        var _CONTENT = CONTRNTBUFFER[0];
        
        return _CONTENT;
    },
    ToDouble : function(arrayBuffer, start, size){
        var _tab = start;
        var CONTRNTBUFFER = new Float64Array(arrayBuffer.slice(start, _tab + size));
        var _CONTENT = CONTRNTBUFFER[0];
        
        return _CONTENT;
    },
    ToStringArray : function(arrayBuffer, start, size, arraylength, index , arr){
        var _tab = start;
        var _STR_LENGTH = new Uint32Array(arrayBuffer.slice(_tab, _tab + size));
        _tab += size;
        var CONTRNTBUFFER = new Uint8Array(arrayBuffer.slice(_tab, _tab + _STR_LENGTH[0]));
        _tab += _STR_LENGTH[0];
        var _CONTENT = String.fromCharCode.apply("", new Uint16Array(CONTRNTBUFFER));
        _CONTENT = decodeURIComponent(escape(_CONTENT));
        arr.push(_CONTENT);
        var _INDEX = index + 1;
        if(_tab <arraylength){
            return WampValueHelper.ToStringArray(arrayBuffer, _tab, size, arraylength, _INDEX, arr);
        }else{
            return arr;
        }
    },
    ToByteArray : function(arrayBuffer, start, size, arraylength, index , arr){
        var _tab = start;
        // var CONTRNTBUFFER = new Uint8Array(arrayBuffer.slice(start, _tab + arraylength));
        // _tab += size;
        // var _CONTENT = new Blob(CONTRNTBUFFER);
        var _CONTENT = new Blob([arrayBuffer.slice(start, _tab + arraylength)], { type: 'image/jpeg' });
        return _CONTENT;
        
    },
    ToBoolArray : function(arrayBuffer, start, size, arraylength, index , arr){
        var _tab = start;
        var CONTRNTBUFFER = new Uint8Array(arrayBuffer.slice(start, _tab + size));
        _tab += size;
        var _CONTENT = CONTRNTBUFFER[0];
        arr.push(_CONTENT);
        var _INDEX = index + 1;
        if(_tab <arraylength){
            return WampValueHelper.ToBoolArray(arrayBuffer, _tab, size, arraylength, _INDEX, arr);
        }else{
            return arr;
        }
    },
    ToIntArray : function(arrayBuffer, start, size, arraylength, index , arr){
        var _tab = start;
        var CONTRNTBUFFER = new Int32Array(arrayBuffer.slice(start, _tab + size));
        _tab += size;
        var _CONTENT = CONTRNTBUFFER[0];
        arr.push(_CONTENT);
        var _INDEX = index + 1;
        if(_tab <arraylength){
            return WampValueHelper.ToIntArray(arrayBuffer, _tab, size, arraylength, _INDEX, arr);
        }else{
            return arr;
        }
    },
    ToShortArray : function(arrayBuffer, start, size, arraylength, index , arr){
        var _tab = start;
        var CONTRNTBUFFER = new Int32Array(arrayBuffer.slice(start, _tab + size));
        _tab += size;
        var _CONTENT = CONTRNTBUFFER[0];
        arr.push(_CONTENT);
        var _INDEX = index + 1;
        if(_tab <arraylength){
            return WampValueHelper.ToIntArray(arrayBuffer, _tab, size, arraylength, _INDEX, arr);
        }else{
            return arr;
        }
    },
    ToUShortArray : function(arrayBuffer, start, size, arraylength, index , arr){
        var _tab = start;
        var CONTRNTBUFFER = new Uint16Array(arrayBuffer.slice(start, _tab + size));
        _tab += size;
        var _CONTENT = CONTRNTBUFFER[0];
        arr.push(_CONTENT);
        var _INDEX = index + 1;
        if(_tab <arraylength){
            return WampValueHelper.ToIntArray(arrayBuffer, _tab, size, arraylength, _INDEX, arr);
        }else{
            return arr;
        }
    },
    ToFloatArray : function(arrayBuffer, start, size, arraylength, index , arr){
        var _tab = start;
        var CONTRNTBUFFER = new Float32Array(arrayBuffer.slice(start, _tab + size));
        _tab += size;
        var _CONTENT = CONTRNTBUFFER[0];
        arr.push(_CONTENT);
        var  _INDEX = index + 1;
        if(_tab <arraylength){
            return WampValueHelper.ToFloatArray(arrayBuffer, _tab, size, arraylength, _INDEX, arr);
        }else{
            return arr;
        }
    },
    ToDoubleArray : function(arrayBuffer, start, size, arraylength, index , arr){
        var _tab = start;
        var CONTRNTBUFFER = new Float64Array(arrayBuffer.slice(start, _tab + size));
        _tab += size;
        var _CONTENT = CONTRNTBUFFER[0];
        arr.push(_CONTENT);
        var _INDEX = index + 1;
        if(_tab <arraylength){
            return WampValueHelper.ToDoubleArray(arrayBuffer, _tab, size, arraylength, _INDEX, arr);
        }else{
            return arr;
        }
    },
    ToJSONArray : function(arrayBuffer, start, size, arraylength, index , arr){
        var _tab = start;
        var _STR_LENGTH = new Uint32Array(arrayBuffer.slice(_tab, _tab + size));
        _tab += size;
        var CONTRNTBUFFER = new Uint8Array(arrayBuffer.slice(_tab, _tab + _STR_LENGTH[0]));
        _tab += _STR_LENGTH[0];
        var _CONTENT = String.fromCharCode.apply("", new Uint16Array(CONTRNTBUFFER));
        _CONTENT = decodeURIComponent(escape(_CONTENT));
        arr.push(JSON.parse(_CONTENT));
        var _INDEX = index + 1;
        if(_tab <arraylength){
            return WampValueHelper.ToJSONArray(arrayBuffer, _tab, size, arraylength, _INDEX, arr);
        }else{
            return arr;
        }
    }
}
export default WampValueHelper;