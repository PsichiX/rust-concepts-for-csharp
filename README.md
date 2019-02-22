# Rust concepts for C# devs - oversimplified

## Introduction

This document is an attempt to show how some safety rules of Rust can be applied
to C# systems development.
Do not take it as something that _must_ be applied - take it as some patterns
that you _may_ consider to use if your use case can gain some profit from it.

__NOTE 1:__ _I have a problem with communicating my thoughts clearly, so: If you
find some typo, something confusing or simply just not true, please create an
issue or pull request with changes and explanation - i will be more than happy
for that :)_

__NOTE 2:__ _This document definitely does not serve as attempt to convert any
C# dev into Rust - author just uses Rust as a tool for showing how safety may be
obtained with Rust concepts as an example. Please, do not hate him for doing
that :3_

## Ownership, lifetimes and borrowing

The most important thing about Rust ownership is that any data can be owned only
by one variable at any time. That is: at any given point you cannot have more
than one variable that manage lifetime of given data.
In many languages (if not all) that concept is called __RAII__ (Resource
Acquisition Is Initialization) but it is limited there only to resources
("heavy" objects that may contain native / hidden data that needs to be released
after use) - for example:
- files;
- sockets;
- unmanaged memory;
- external code that can be executed (DLLs);
- etc.

First things first - we have to start with basics so there will be a lot of
things that you - C# dev - already know and understand well, but things are
gonna gradualy get bit by bit more complex so please, bare with me.

### Ownership

In case of Rust it treats every data as "resource" and limits its usage to scope
where it was created (in most cases, but more about it later).
Take a look at Rust example of applied single owner - single responsibility
mechanism:
```rust
#[derive(Debug)]
struct Bar {}

fn main() {
  // create `Bar` object and `bar` variable takes ownership of it.
  let bar = Bar {};
  // do something with data owned by `bar`.
  println!("{:?}", bar);
  // assign `Bar` object to another variable, which mean we transfer
  // its ownership to another variable.
  let bar2 = bar;
  // here compiler will tell us that we cannot use `bar` because its value has
  // been moved to another variable.
  println!("{:?}", bar);
}
```

While the same example in C# will not cause any trouble because ownership there
is not moved - it is shared by references. And underlying object will not be
released until all variables that holds reference to it will be out of their
scopes:

```csharp
class Bar { }

public static void Main()
{
  // create `Bar` object and `bar` variable takes reference to it.
  var bar = new Bar();
  // do something with object.
  Console.WriteLine(bar);
  // share ownership to `Bar` object by sharing reference with another variable.
  var bar2 = bar;
  // here we do something with same object and there is no problem with that
  // because object is now shared between two variables.
  Console.WriteLine(bar);
  // and here our object finally may be released because `bar` and `bar2`
  // variables goes out of scope and object has no references pointing into it -
  // garbage collector may free our `Bar` object!
}
```

Okey, so next thing on our basics list to discuss will be passing ownership from
scope to scope - in C# we can simply share object's ownership by returning it
from current function like this:

```csharp
static Bar MakeBar()
{
  // create `Bar` object and share its ownership with `bar` variable.
  var bar = new Bar();
  // do something with object.
  Console.WriteLine(bar);
  // share ownership to `Bar` by passing its reference out of function that
  // creates it.
  return bar;
}

public static void PassingObjectFromScopeToScope()
{
  // here we have got ownership of `Bar` object from function that creates it.
  var bar = MakeBar();
  // and now we can do something else with `Bar`.
  Console.WriteLine(bar);
}
```

You can see that at the end there is *only one* reference to `Bar` object so we
can think of it as ownership being moved, not shared in that case.

The same rule applies for Rust:
```rust
fn make_bar() -> Bar {
  // create `Bar`.
  let bar = Bar {};
  // do something with it.
  println!("{:?}", bar);
  // return `Bar` with ownership out of function.
  bar
}

pub fn passing_object_from_scope_to_scope() {
  // produce `Bar` and take it with ownership.
  let bar = make_bar();
  // do another thing with it.
  println!("{:?}", bar);
}
```

### Borrowing

Rust introduces concept of borrowing, which is: _access to data is given by
reference so no ownership is passed anywhere_. And its shorthand rule tells
that you can have one of two kinds of borrow at time but not both:
- You can have as __many immutable__ references as you want;
- You can have at most only __one mutable__ reference;

These rules solves problem of data races, where many writers modify data at the
same time. Take a look how it can be done with both languages - Rust first:
```rust
#[derive(Debug)]
struct Bar2 {
    pub answer: i32,
}

// object `Bar2` is borrowed immutably so only thing we can do with it is read.
// also because it is borrowed, its data is not released at the end of function.
fn immutable_borrow(bar: &Bar2) {
  // we do something with data.
  println!("{:?}", bar);
  // compile-time error: we cannot write to data through immutable reference.
  // bar.answer = 40;
}

// object `Bar2` is borrowed mutably so we can modify its data.
fn mutable_borrow(bar: &mut Bar2) {
  // here we mutate `Bar2` data.
  bar.answer = 40;
  println!("{:?}", bar);
}

pub fn borrowing() {
  // create `Bar2` data and mark `bar` variable as owner of data that can be
  // modified later.
  let mut bar = Bar2 { answer: 42 };
  immutable_borrow(&bar);
  mutable_borrow(&mut bar);
}
```

Now immutable access in C#:
```csharp
struct Bar2
{
  public int answer;
}

// notice this `in` keyword - it comes along with C# 7.2 features and it tells
// compiler that data is passed as readonly pointer so all we can do with data
// under `bar` variable is to read it and no motation will be allowed.
// but remember: it works only with structs - with classes, compiler will not
// throw an error and you will be allowed to mutate data even if it is marked
// with `in` keyword.
static void ImmutableBorrow(in Bar2 bar)
{
  Console.WriteLine(bar.answer);
  // compile-time error: cannot modify because `bar` is readonly.
  //bar.answer = 40;
}

// while data passed by reference is mutable by default.
static void MutableBorrow(ref Bar2 bar)
{
  bar.answer = 40;
  Console.WriteLine(bar.answer);
}

public static void Borrowing()
{
  var bar = new Bar2
  {
      answer = 42
  };
  ImmutableBorrow(bar);
  MutableBorrow(ref bar);
}
```

But hey, why should i care about immutables in C# when i always worked with
mutables on default? Consider some example of premature data release:

```csharp
// `Bar3` has some internal resource that is managed by some external entity,
// so  while `Bar3` owns its internal state, something still may mutate it.
struct Bar3
{
  public List<int> data;
}

// this function borrows mutably `Bar3` and accidentialy releases its internals.
static void DoSomethingCrashy(ref Bar3 bar, int state)
{
  Console.WriteLine(bar.data[i]);
  if (state > 0) {
    bar.data = null;
  }
}

public static void PrematureDataRelease()
{
  var bar = new Bar3
  {
    data = new List<int>
    {
      1, 2, 3
    }
  };
  // perform iterations over object internals to process them and then..
  // run-time error: null pointer at third iteration
  // because of `Bar3` data mutation in second iteration.
  for (var i = 0; i < bar.data.Count; ++i)
  {
    // and the source of trouble is here - purpouse of this loop was to process
    // data in read only manner but as time goes by, at some point, function
    // that process data starts to mutate owner of them.
    DoSomethingCrashy(ref bar, i);
  }
}
```

Example above is oversimplified but any code sooner or later gets so much
complicated, at some point in time, in some place deep in stack
trace there will be a function call that mutates something that should be only
read, not mutated and error with internal state of data in another place will
occur and will be hard to locate (i guess anyone at least once in their life had
to debug a lot of calls chain to find source of that problem).

So what exactly we can do to avoid that kind of situation? We can enforce
immutable functions to allow only immutable data, like that:

```csharp
// this function can only read data from `Bar3` so it is safe to use within
// immutable data processing pipeline.
static void CannotDoSomethingCrashy(in Bar3 bar, int state)
{
  Console.WriteLine(bar.data[state]);
  if (state > 0)
  {
    // compile-time error: cannot modify because `bar` is readonly.
    //bar.data = null;
  }
}

public static void PrematureDataRelease()
{
  var bar = new Bar3
  {
    data = new List<int>
    {
        1, 2, 3
    }
  };
  for (var i = 0; i < bar.data.Count; ++i)
  {
    CannotDoSomethingCrashy(bar, i);
  }
}
```

Readonly pointers may not solve all your problems
with accidential mutability, but they are good at least at forcing scope to not
mutate input data.

### Lifetimes
`TODO`

### Summary
`TODO`

## Data structures
`TODO`
