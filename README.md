# ScriptCore
A .NET Standard 2.0 MoonSharp wrapper library for easy binding and quickly running synchronous Lua scripts. Extended Coroutine functionality is also added

Readme under development
<br/>
All ScriptRunners currently have MoonSharp sandboxed (CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time)
<br/>
Sandboxing options will be added at a later date
<h2>Quick Start</h2>

<h3>The Basics</h3>

```csharp
using System;
using ScriptCore;

class Program
{
  static void Main(string[] args)
  {
    //Register functions marked with the LuaFunction attribute
    GlobalScriptBindings.HookClasses(typeof(Program));

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
  [LuaFunction("HelloWorld")]
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
using ScriptCore;

class Program
{
  static void Main(string[] args)
  {
    GlobalScriptBindings.HookClasses(typeof(Program));
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

  [LuaFunction("AddCS")]
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
using ScriptCore;
class Program
{
  static void Main(string[] args)
  {
    GlobalScriptBindings.HookClasses(typeof(Program));

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

  [LuaFunction("HelloWorld")]
  static void PrintHelloWorld()
  {
    Console.WriteLine("Hello World!");
  }
}

class ExampleClass
{
  public int MyNumber { get; set; } = 0;

  [LuaFunction("PrintMyNumber")]
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
using ScriptCore;

class Program
{
  static void Main(string[] args)
  {
    GlobalScriptBindings.RegisterNewableType(typeof(ExampleClass));
    GlobalScriptBindings.RegisterStaticType(typeof(ExampleStaticClass));
    HookedScriptRunner runner = new HookedScriptRunner();

    runner.LoadScript(
      "function foo(exClassIn) " +
      "  exClassIn.PrintMyNumber()" + 
      "  ExampleClass(6).PrintMyNumber()" +
      "  _ExampleStaticClass.MyNumber = 3" +
      "  _ExampleStaticClass.StaticPrintMyNumber()" +
      "end " +
      "RegisterHook(foo,'Foo')"
      );

    runner.Execute("Foo", new ExampleClass(5));
    Console.ReadKey();
  }
}

class ExampleClass
{
  public int MyNumber { get; private set; }

  public ExampleClass(int myNumber)
  {
    MyNumber = myNumber;
  }

  public void PrintMyNumber()
  {
    Console.WriteLine($"My Number is: {MyNumber}");
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
ScriptCore has an extended coroutine system, similar to Unity's <br/>
There is one built in Yielder, WaitFrames. WaitFrames yields and then waits X amount of frames before allowing continuation <br/>
The Yielder class can be extended and Yielder subclasses can be registered for use in Lua <br/>
When hooking a C# function in GlobalScriptBindings or ScriptBindings, any hooked method that returns a subclass of Yielder will automatically be wrapped in a coroutine.yield() statement.

```csharp
using System;
using ScriptCore;
using ScriptCore.Yielding;
class Program
{
  static void Main(string[] args)
  {
    GlobalScriptBindings.HookClasses(typeof(Program));
	//Register custom Yielder class
    Yielders.RegisterYielder<MyYielder>();

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

  [LuaFunction("AutoYieldOneCall")]
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

For use with starting Unity coroutines and continuing only when they are completed, the following technique can be used:

```csharp
[LuaFunction("TestMethod")]
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
  //WaitForDone is an included Yielder class in ScriptCore
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
