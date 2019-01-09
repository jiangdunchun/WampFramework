# Welcome to WampFramework
-------

Author: [Dunchun Jiang](jiangdunchun@outlook.com)

Version: 1.0

Date: 26/11/2018

Here is a [website](https://wamp-proto.org) introducing the theory of **Wamp**. But, actually, I modify some technical details in order to meet the demand of invoking API in local c# assembly. In summary, this framework could do these:
>* Open the API of c# assembly to websocket connection in a simple and quick way, even though you don't have a clear conscious about how to organize the router
>* Export all c# API to js file, as a result, the front_end could invoke these API just by referencing these files
>* The asynchronous invoking is also supported. In other words, even though one method invoked by remote command in your assembly needs a long time to process, the later remote command will not be blocked

## Protocol Standard
This framework supports two communication modes of websocket: **String Mode** and **Byte[] Mode**. Both these two modes have the same items. Before introducing the structure of message, it's essential to know what these items mean.

**Protocol Head**: several numbers are defined to present different command type of this message. This chart explains which command type each number represents:

 Number | Command
 :----: | :----: 
 0 | Remote call 
 8 | Feedback of successful remote call 
 9 | Feedback of failed remote call 
 10 | Remote subscribe 
 11 | Subscribed event invoked 
 18 | Feedback of successful remote subscribe 
 19 | Feedback of failed remote subscribe 
 20 | Remote unsubscribe 
 28 | Feedback of successful remote unsubscribe 
 29 | Feedback of failed remote unsubscribe 
 30 | Remote register 
 38 | Feedback of successful remote register 
 39 | Feedback of failed remote register 
 101 | An error happened 

**ID**: No matter calling a method or subscribing a event, the Wamp slaves need to generate a unique ID, and send it to Wamp master. After that, the master will send it back to slave in the feedback message. This ID make sure that all message could be send back to the position where it came from.

**Class Name**: The name of a class in c# assembly.

**Method(Event) Name**: The name of a method(event) belong to the class mentioned above in c# assembly.

**Argument_n**: Wamp message will have this item in 3 occasions. Firstly, the slave should send the arguments when remotely call a method, if this method needs. Secondly, the master would send a argument back in the feedback message of a remote call, if this method has return value. Finally, the master would send arguments back when a subscribed event invoked , if this event delivers parameters out. 


### String Mode

Every item of message is split by `'|'`. This chart explains the items of **string mode** message(`*`represents this item is not essential) 

Index| Item 
:----: | :----: 
0 | Protocol Head 
1 | ID 
2 | Class Name 
3 | Method(Event) Name 
4`*` | Argument_1 
5`*` | Argument_2 
... | ... 


### Byte[] Mode

Every part of message has length limitation for itself. This chart explains the items of **byte[] mode** message(`*`represents this item is not essential)(`*****`represents this item will be explained latter). 

 Index| Item | Structure| Length 
 :----: | :----: | :----: | :----: 
 0 | Protocol Head | byte | 1 
 1 | ID | UInt16 | 2 
 2 | Class Name and Method(Event) Name | byte+string| 1+n 
 3`*` | Argument_1 | `*****` | `*****` 
 4`*` | Argument_2 | `*****` | `*****`
 ... | ... | ... | ... 

It might look a little strange of the **Type** and **Length** in **Index 2**. Actually, **Index 2** stores the Class Name and Method(Event) Name together, and splits them using a `'|'`, because these two items are both `string` type. Another consideration is we need a `byte` to illustrates the length of this compound string's length. Also for this reason, the length of `byte[]` converted from this compound string must less than 255. So, the **Type** of **Index 2** is `byte+string`, and the **Length** of **Index 2** is `1+n`,  the number of front `byte` is compound string's length.

Another point need to be noticed is the **Type** and **Length** of **Argument_n**. I explain it in more details because it's a bit of complicated. Different types of arguments have different length when they were converted to `byte[]`, so, we need to label which type this argument is. Several numbers(Byte) are defined to present different argument types. This chart explains each type's number.

 Type | Structure | Length | Content
 :----: | :----: | :----: | :----:
  null | byte | 1 | "0"
  string | byte+int+string | 1+4+n | "1"+"string.length"+"string.content"
  byte | byte+byte | 1+1 | "2"+"byte.content"
  bool | byte+byte | 1+1 | "3"+"bool.content"
  int |  byte+int| 1+4 | "4"+"int.content"
  float |  byte+float| 1+4 | "5"+"float.content"
  double |  byte+double | 1+8 | "6"+"float.content"
  string[] | byte+int+int+string+... |  1+4+{(4+n1)+...} | "11"+"array.length"+"s1.length"+"s1.content"+...
  byte[] | byte+int+byte+... | 1+4+{1+...} | "12"+"array.length"+"b1.content"+...
  bool[] | byte+int+bool+... | 1+4+{1+...} | "13"+"array.length"+"b1.content"+...
 int[] | byte+int+int+... | 1+4+{4+...} | "14"+"array.length"+"i1.content"+...
 float[] | byte+int+float+... | 1+4+{4+...} | "15"+"array.length"+"f1.content"+...
 double[] | byte+int+double+... | 1+4+{8+...} | "16"+"array.length"+"d1.content"+...
 json | byte+int+json+... |  1+4+n | "255"+"json.length"+"json.content"

From what has been discussed above, it's clear why the **Type** of **Index 3**(**Index 4**) is `byte+int(double)`, and the **Length** of **Index 3**(**Index 4**) is `1+4(8)`. Number of front byte presents this argument's type, and the next `4`(`8`) bytes store the real argument. 

But we still have a problem in how to know this item's length when it was converted to `byte[]` if its type is `string` or `byte[]`. We use the same way as **Index 2**, inserting a new `int` to present the length. The reason of using an `int` to label the length rather than a `byte`, is the argument might be pixel values of a texture sometimes(Imagining you are sending a RGB256 texture whose size is `1920*1080`, the byte array's legth will be `1920*1080*3`. In this case, a byte is not enough to present the length). As a result, the **Type** of **Index 5**(**Index 6**) is `byte+int+string(byte[])`, and the **Length** of **Index 5**(**Index 6**) is `1+4+n`. Number of front `byte` presents this argument's type, the next `int` presents the length, and the last `n` bytes presents the real argument. 




