# RemCon
A demonstrator project which illustrates the process of sending a typed object via TCP/IP in C# and provides a simple to use data transfer library.

The purpose of this project is to demonstrate the process of sending a typed object via TCP/IP in C#. In addition, a useful "*drop-in*" library has been created which makes it simple to add such transfer functionality to any C# project. In other words, you can create a typed object with whatever data fields you wish and then send it across the connection where it appears at the other end as an instantiated object of the same type. The mechanics of the transfer are irrelevant to either the sending or receiving application and the data is exchanged transparently. 

The project includes a demonstration Server and a Client which exchange data as a typed object. The Client software will function both in Windows and on a Linux box under Mono. The Server has only been tested on Windows 10 but will probably work under Mono 

The RemCon Sample Applications are open source and released under the MIT License. The home page for this project can be found at [http://www.OfItselfSo.com/RemCon](http://www.OfItselfSo.com/RemCon).

