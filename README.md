# LunarBind
A .NET Standard 2.0 MoonSharp wrapper library for easy binding and quickly running synchronous Lua scripts.<br/>
Extended Coroutine functionality is also added, including custom Yielders which are similar in functionality to Unity's YieldInstructions

Library is currently under development.<br/>
Readme also under development, may have errors in examples.<br/>
All ScriptRunners currently have MoonSharp sandboxed (CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time)
<br/>
Sandboxing options will be added at a later date
<h2>Quick Start</h2>

<h3>The Basics</h3>

```csharp
using System;
using LunarBind;

class Program
{
  static void Main(string[] args)
  {
    //Register functions marked with the LunarBindFunction attribute
    GlobalScriptBindings.AddTypes(typeof(Program));

    //All runners are initialized with global script bindings on creation
    HookedScriptRunner runner = new HookedScriptRunner();

    //Load a script. RegisterHook registers a MoonSharp function that can be called from C# in your script runner
    runner.LoadScript(
      "function foo() " +
      "  HelloWorld()" + 
      "end " +
      "RegisterHook(foo,'Foo')"
      );

    //Run the lua function
    runner.Execute("Foo");
    Console.ReadKey();
  }

  //Mark a static method as a function that can be called from MoonSharp
  [LunarBindFunction("HelloWorld")]
  static void PrintHelloWorld()
  {
    Console.WriteLine("Hello World!");
  }
}
```

<h3>Applying Standards</h3>

```csharp
using System;
using LunarBind;
using LunarBind.Standards;
class Program
{
  static void Main(string[] args)
  {
    GlobalScriptBindings.AddTypes(typeof(Program));

    //Create a standard that any script must follow
    LuaScriptStandard standard = new LuaScriptStandard(
      new LuaFuncStandard(path: "Foo", isCoroutine: false, autoResetCoroutine: false, required: true)
      );
    HookedScriptRunner runner = new HookedScriptRunner(standard);

    //No error
    runner.LoadScript(
      "function Foo() " +
      "  HelloWorld()" +
      "end "
      );
    runner.Execute("Foo");

    //No error
    runner.LoadScript(
      "function Foo() " +
      "  print('Hello to you too!')" +
      "end "
      );
    runner.Execute("Foo");

    //Throws error on loading
    runner.LoadScript(
      "function Bar() " +
      "  HelloWorld()" +
      "end "
      );

    runner.Execute("Foo");

    Console.ReadKey();
  }

  [LunarBindFunction("HelloWorld")]
  static void PrintHelloWorld()
  {
    Console.WriteLine("Hello World!");
  }
}
```

<h3>Querying Results</h3>
Note: Only supported in HookedScriptRunner and BasicScriptRunner

```csharp
using System;
using LunarBind;

class Program
{
  static void Main(string[] args)
  {
    GlobalScriptBindings.AddTypes(typeof(Program));
    HookedScriptRunner runner = new HookedScriptRunner();
    runner.LoadScript(
      "function foo(a, b) " +
      "  return AddCS(a, b)" + 
      "end " + 
      "RegisterHook(foo,'Foo')"
      );

    int result = runner.Query<int>("Foo", 2, 3);
    Console.WriteLine($"The result is: {result}");
    Console.ReadKey();
  }

  [LunarBindFunction("AddCS")]
  static int Add(int a, int b)
  {
    return a + b;
  }
}
```

<h3>Local ScriptBindings and Instance Methods</h3>
Note: it is highly recommended to only bind instances to ScriptBindings and not GlobalScriptBindings

```csharp
using System;
using LunarBind;

class Program
{
  static void Main(string[] args)
  {
    GlobalScriptBindings.AddTypes(typeof(Program));

    ExampleClass instanceObject = new ExampleClass() { MyNumber = 27 };

    ScriptBindings bindings = new ScriptBindings(instanceObject);

    HookedScriptRunner runner = new HookedScriptRunner(bindings);

    runner.LoadScript(
      "function foo() " +
      "  HelloWorld()" + 
      "  PrintMyNumber()" +
      "end " +
      "RegisterHook(foo,'Foo')"
      );

    runner.Execute("Foo");
    Console.ReadKey();
  }

  [LunarBindFunction("HelloWorld")]
  static void PrintHelloWorld()
  {
    Console.WriteLine("Hello World!");
  }
}

class ExampleClass
{
  public int MyNumber { get; set; } = 0;

  [LunarBindFunction("PrintMyNumber")]
  private void PrintMyNumber()
  {
    Console.WriteLine($"My Number is: {MyNumber}");
  }
}
```
<h3>Registering and Using Types</h3>
Creating new instances of registered types has been simplified (an option to use original "._new()" syntax will be added)
<br/>
Accessing a type's static functions and variables currently requires appending "_" to the type name
<br/>
Use MoonSharp's [UserData](https://www.moonsharp.org/objects.html) attributes to hide functions and members 

```csharp
using System;
using LunarBind;

class Program
{
  static void Main(string[] args)
  {
    //Allows you to use the type name as a constructor (.__new()), 
    //but you must prefix the type with GlobalScriptBindings.TypePrefix to use static methods and fields
    GlobalScriptBindings.AddNewableType(typeof(ExampleClass1));

    //Registers type as UserData and adds it to the script's Global table
    GlobalScriptBindings.AddGlobalType(typeof(ExampleClass2));
    GlobalScriptBindings.AddGlobalType(typeof(ExampleStaticClass));

    HookedScriptRunner runner = new HookedScriptRunner();

    runner.LoadScript(
      "function foo(exClassIn) " +
      "  exClassIn.PrintMyNumber()" +
      "  ExampleClass1(6).PrintMyNumber()" +       //Newing using newable syntax
      "  print(_ExampleClass1.StaticVariable)" +   //Accessing newable statics 
      "  ExampleClass2.__new(7).PrintMyNumber()" + //Newing using default Lua syntax
      "  print(ExampleClass2.StaticVariable)" +    //Accessing default Lua syntax statics
      "  ExampleStaticClass.MyNumber = 3" +
      "  ExampleStaticClass.StaticPrintMyNumber()" +
      "end " +
      "RegisterHook(foo,'Foo')"
      );

    runner.Execute("Foo", new ExampleClass1(5));
    Console.ReadKey();
  }
}

class ExampleClass1
{
  public static readonly int StaticVariable = 1;

  public int MyNumber { get; private set; }

  public ExampleClass1(int myNumber)
  {
    MyNumber = myNumber;
  }

  public void PrintMyNumber()
  {
    Console.WriteLine($"[Example Class 1] My Number is: {MyNumber}");
  }
}

class ExampleClass2
{
  public static readonly int StaticVariable = 2;

  public int MyNumber { get; private set; }

  public ExampleClass2(int myNumber)
  {
    MyNumber = myNumber;
  }

  public void PrintMyNumber()
  {
    Console.WriteLine($"[Example Class 2] My Number is: {MyNumber}");
  }
}

static class ExampleStaticClass
{
  public static int MyNumber = 42;

  public static void StaticPrintMyNumber()
  {
    Console.WriteLine($"My Static Number is: {MyNumber}");
  }
}
```

<h3>Coroutines</h3>
Coroutines are supported on HookedScriptRunner and HookedStateScriptRunner <br/>
LunarBind has an extended coroutine system, similar to Unity's <br/>
There is one built in Yielder, WaitFrames. WaitFrames yields and then waits X amount of frames before allowing continuation <br/>
The Yielder class can be extended and Yielder subclasses can be registered for use in Lua <br/>
When hooking a C# function in GlobalScriptBindings or ScriptBindings, any hooked method that returns a subclass of Yielder will automatically be wrapped in a coroutine.yield() statement.

```csharp
using System;
using LunarBind;
using LunarBind.Yielding;
class Program
{
  static void Main(string[] args)
  {
    GlobalScriptBindings.AddTypes(typeof(Program));
    //Register custom Yielder class. all Yielder classes must be created with YielderTypeName() in Lua
    GlobalScriptBindings.AddYieldableType<MyYielder>();

    HookedScriptRunner runner = new HookedScriptRunner();
    string script =
      "function foo(callNumber) " +
      "  print('Call '..tostring(callNumber)..', yielding 0 calls with WaitFrames')" +
      "  local r = coroutine.yield(WaitFrames(0))" +
      "  print('Call '..tostring(r)..', yielding 1 calls with auto yielder')" +
      "  r = AutoYieldOneCall()" +
      "  print('Call '..tostring(r)..', yielding 3 calls with MyYielder')" +
      "  r = coroutine.yield(MyYielder())" + //Must be constructed like MyYielder()
      "  print('Call '..tostring(r)..', done. Coroutine is now dead')" +
      "  " +
      "end " +
      "RegisterCoroutine(foo,'FooCoroutine')"; //RegisterCoroutine instead of RegisterHook

    Console.WriteLine("====================== Coroutine ======================");
    runner.LoadScript(script);    
    for (int i = 0; i < 10; i++)
    {
      Console.WriteLine();
      Console.WriteLine($"[C#: Call {i + 1}]");
      runner.Execute("FooCoroutine", (i+1));
    }

    Console.WriteLine();
    Console.WriteLine("====================== Coroutine Complete Callback ======================");
    //Reload the script
    runner.LoadScript(script);
    for (int i = 0; i < 10; i++)
    {
      Console.WriteLine();
      Console.WriteLine($"[C#: Call {i + 1}]");
      runner.ExecuteWithCallback("FooCoroutine", () => { i = 10; Console.WriteLine("Callback On Done, exiting loop"); },  (i + 1));
    }

    Console.ReadKey();
  }

  [LunarBindFunction("AutoYieldOneCall")]
  static Yielder AutoYieldOneCall()
  {
    return new WaitFrames(1);
  }

}

class MyYielder : Yielder
{
  int yieldCountDown = 3;
  public override bool CheckStatus()
  {
    return yieldCountDown-- <= 0;
  }
}
```

For use with starting Unity coroutines from Lua and continuing only when they are completed, the following technique can be used:

```csharp
[LunarBindFunction("TestMethod")]
Yielder TestMethod(string text)
{
  return this.RunUnityCoroutineFromLua(MyUnityCoroutine(text));
}

//Unity coroutine
IEnumerator MyUnityCoroutine(string text)
{
  yield return new WaitForSeconds(2);
  Debug.Log(text);
  yield return new WaitForSeconds(2);
  Debug.Log(text);
  yield return new WaitForSeconds(2);
}

//Implement in an extension class
public static WaitForDone RunUnityCoroutineFromLua(this MonoBehaviour behaviour, IEnumerator toRun)
{
  //WaitForDone is an included Yielder class in LunarBind
  var yielder = new WaitForDone();
  IEnumerator Routine(WaitForDone waitForDone)
  {
    yield return toRun;
    waitForDone.Done = true;
  }
  behaviour.StartCoroutine(Routine(yielder));
  return yielder;
}
```

You can also create a Unity coroutine from any ScriptFunction, or through HookedScriptRunner<br/>
These coroutines will yield return null on yielding or forced yields (moonsharp coroutine auto yield)

```csharp
using System;
using LunarBind;

class MyMonoBehaviour : MonoBehaviour
{
  void Start()
  {
    //Only set up GlobalScriptBindings once in a static initializer class
    GlobalScriptBindings.AddTypes(typeof(MyMonoBehaviour));
    //Register custom Yielder class
    GlobalScriptBindings.AddYieldableType<MyYielder>();
    
    HookedScriptRunner runner = new HookedScriptRunner();
    string script =
      "function foo(callNumber) " +
      "  print('Call '..tostring(callNumber)..', yielding 0 calls with WaitFrames')" +
      "  local r = coroutine.yield(WaitFrames(0))" +
      "  print('Call '..tostring(r)..', yielding 1 calls with auto yielder')" +
      "  r = AutoYieldOneCall()" +
      "  print('Call '..tostring(r)..', yielding 3 calls with MyYielder')" +
      "  r = coroutine.yield(MyYielder())" +
      "  print('Call '..tostring(r)..', done. Coroutine is now dead')" +
      "  " +
      "end " +
      "RegisterCoroutine(foo,'FooCoroutine')"; //RegisterCoroutine instead of RegisterHook
    runner.LoadScript(script);
    
    var func = runner.GetFunction("FooCoroutine");
    StartCoroutine(func.AsUnityCoroutine(3));
    StartCoroutine(runner.CreateUnityCoroutine("FooCoroutine", 3));
    }
    
    [LunarBindFunction("AutoYieldOneCall")]
    static Yielder AutoYieldOneCall()
    {
      return new WaitFrames(1);
    }

}

class MyYielder : Yielder
{
  int yieldCountDown = 3;
  public override bool CheckStatus()
  {
    return yieldCountDown-- <= 0;
  }
}
```

<h2>Using your own Scripts</h2>

You can use the binding functionality of LunarBind on any moonsharp Script object. 

```csharp
using System;
using LunarBind;
using LunarBind.Yielding;
using MoonSharp.Interpreter;
class Program
{
  static void Main(string[] args)
  {
    GlobalScriptBindings.AddTypes(typeof(Program));
    GlobalScriptBindings.AddYieldableType<MyYielder>();

    Script script = new Script();
    string scriptString = "function foo(callNumber) " +
      "  print('Call '..tostring(callNumber)..', yielding 0 calls with WaitFrames')" +
      "  local r = coroutine.yield(WaitFrames(0))" +
      "  print('Call '..tostring(r)..', yielding 1 calls with auto yielder')" +
      "  r = AutoYieldOneCall()" +
      "  print('Call '..tostring(r)..', yielding 3 calls with MyYielder')" +
      "  r = coroutine.yield(MyYielder())" +
      "  print('Call '..tostring(r)..', done. Coroutine is now restarting')" +
      "  " +
      "end ";

    //Initialize the script
    GlobalScriptBindings.Initialize(script);
	//Run the string
    script.DoString(scriptString);

    //Create a ScriptFunction targeting a global function named "foo", as a coroutine
    bool isCoroutine = true;
    bool autoResetCoroutine = true;
    ScriptFunction func = new ScriptFunction("foo", script, isCoroutine, autoResetCoroutine);
    
    for (int i = 0; i < 16; i++)
    {
      Console.WriteLine();
      Console.WriteLine($"[C#: Call {i + 1}]");
      func.Execute(i+1);
    }

    Console.ReadKey();
  }

  [LunarBindFunction("AutoYieldOneCall")]
  static Yielder AutoYieldOneCall()
  {
    return new WaitFrames(1);
  }
}

class MyYielder : Yielder
{
  int yieldCountDown = 3;
  public override bool CheckStatus()
  {
    return yieldCountDown-- <= 0;
  }
}
```
