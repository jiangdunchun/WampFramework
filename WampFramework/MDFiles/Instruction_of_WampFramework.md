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
This framework supports two message modes of websocket: **String Mode** and **Byte[] Mode**. Both these two modes have the same contents. Before introducing the structure of message, it's essential to know what these items mean.

**Protocol Head**: several numbers are defined to present different means of this message. This chart explains which mean each number represents:

 Number| Mean
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

**ID**: No matter calling a method or subscribing a event, the Wamp slaves need to generate a unique ID, and send it to Wamp master. After that, the master will send it back to slave in the feedback message of this calling or subscribing. This ID make sure that all message could be send back to the place where it came from.

**Class Name**: The name of a class in c# assembly.

**Method(Event) Name**: The name of a method(event) belong to the class mentioned above in c# assembly.

**Argument_n**: Wamp message will have this item in 3 occasions. Firstly, the slave should send the arguments when remotely call a method, if this method needs. Secondly, the master would send a argument back in the feedback of a remote call, if this method has return value. Finally, the master would send arguments back when a subscribed event invoked , if this event delivers parameters out. 


### String Mode

Every item of message is split by `','`. This chart explains the content of string mode message(`*`represents this item is not essential) 

Index| Content 
:----: | :----: 
0 | Protocol Head 
1 | ID 
2 | Class Name 
3 | Method(Event) Name 
4`*` | Argument_1 
5`*` | Argument_2 
... | ... 


### Byte[] Mode

Every part of message has length limit for itself. This chart explains the content of byte[] mode message(`*`represents this item is not essential). 

 Index| Content | Type| Length 
 :----: | :----: | :----: | :----: 
 0 | Protocol Head | byte | 1 
 1 | ID | UInt16 | 2 
 2 | Class Name,Method(Event) Name | byte+string| 1+n 
 3`*` | Argument_1 | byte+int | 1+4 |
 4`*` | Argument_2 | byte+double | 1+8 
 5`*` | Argument_3 | byte+int+string | 1+4+n 
 6`*` | Argument_4 | byte+int+byte[] | 1+4+n 
 ... | ... | ... | ... 

It might look a little strange of the **Type** and **Length** in **Index 2**. Actually, **Index 2** stores the Class Name and Method(Event) Name together, and splits them using a `','`, because these two items are both string type. Another consideration is we need a byte to presents the length of this combine string's length. Also for this reason, the length of `byte[]` transfered from this combine string must less than 255. So, the **Type** of **Index 2** is `byte+string`, and the **Length** of **Index 2** is `1+n`,  the number of front byte is combine string's length.

Another point need to be noticed is the **Type** and **Length** of **Argument_n**. I explain it in more details because it's a bit of complicated. Different types of arguments have different length when they transfer to `byte[]`, so, we need to label which type this argument is. Several numbers are defined to present different argument types. This chart explains each type's number(in order to contain majority of types in c#, I define the argument types like `byte`  `UInt16` are both `int`, and `float` is also `double`)

 Number| Type 
 :----: | :----: 
 0 | null
 1 | string 
 2 | byte[] 
 4 | int 
 8 | double 

From what has been discussed above, it's clear why the **Type** of **Index 3**(**Index 4**) is `byte+int(double)`, and the **Length** of **Index 3**(**Index 4**) is `1+4(8)`. Number of front byte presents this argument's type, and the next `4`(`8`) bytes presents the real argument. 

But we still have a problem in how to know this item's length when it transfers to `byte[]` if its type is `string` or `byte[]`. We use the same way to **Index 2**, inserting a new `int` to present the length. The reason of using an `int` to label the length rather than a `byte`, is the argument might be pixel value of a texture sometimes. As a result, the **Type** of **Index 5**(**Index 6**) is `byte+byte+string(byte[])`, and the **Length** of **Index 5**(**Index 6**) is `1+1+n`. Number of front byte presents this argument's type, the next byte presents the length, and the last `n` bytes presents the real argument. 




