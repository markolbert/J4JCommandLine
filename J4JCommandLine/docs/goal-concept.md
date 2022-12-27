# Goal and Concept

There are a number of C# command line parsers out there, several of which I've used. So why write another one?

Well, in part because, for me at least, I only really understanda library after I've (re)written it. I was intrigued by some of the design issues I learned about from Jon and wanted to get a better feel for how to implement them.

I also found most of the available open-source libraries somewhat confusing.That's almost certainly in part the result of not having written them myself. But it seemed like there might be use for the different approach I've come up with here.

While I initially wrote this library to be mostly independent of the Net5 `IConfiguration` system I quickly came to realize that wasn't a good design choice. `IConfiguration` is too pervasive, rightfully so, and too useful to ignore. The library now integrates pretty simply with the `IConfiguration` subsystem.

I also originally wanted the library to work with dependency injection. But after using it for a bit with Net5's `IHostBuilder` system I realized there was what to me looked like a fundamental incompatibility. The `IHostBuilder` system's goal is to establish the infrastucture for your program, *including the dependency injection subsystem*. Having a command line processor which uses dependency injection to work while you're setting up dependency injection is a chicken-and-egg problem. So the redesigned `J4JCommandLine` library doesn't use dependency injection.
