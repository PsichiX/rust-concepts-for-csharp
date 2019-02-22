mod ownership_lifetimes_and_borrowing;

use crate::ownership_lifetimes_and_borrowing::*;

fn main() {
    println!("\n= ownership in a nutshell =");
    ownership_in_a_nutshell();

    println!("\n= passing object from scope to scope =");
    passing_object_from_scope_to_scope();

    println!("\n= borrowing =");
    borrowing();
}
