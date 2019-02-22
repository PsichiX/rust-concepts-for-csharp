#[derive(Debug)]
struct Bar {}

pub fn ownership_in_a_nutshell() {
    let bar = Bar {};
    println!("{:?}", bar);
    let _bar2 = bar;
    // compile-time error: value moved.
    // println!("{:?}", bar);
}

fn make_bar() -> Bar {
    let bar = Bar {};
    println!("{:?}", bar);
    bar
}

pub fn passing_object_from_scope_to_scope() {
    let bar = make_bar();
    println!("{:?}", bar);
}

#[derive(Debug)]
struct Bar2 {
    pub answer: i32,
}

fn immutable_borrow(bar: &Bar2) {
    println!("{:?}", bar.answer);
    // compile-time error: cannot write to data through immutable reference.
    // bar.answer = 40;
}

fn mutable_borrow(bar: &mut Bar2) {
    bar.answer = 40;
    println!("{:?}", bar.answer);
}

pub fn borrowing() {
    let mut bar = Bar2 { answer: 42 };
    immutable_borrow(&bar);
    mutable_borrow(&mut bar);
}
