from sys import argv, stderr, exit
from wasmer import engine, Module, wat2wasm, Store, Instance
from wasmer_compiler_cranelift import Compiler
from falaklib import make_import_object


def main():
    store = Store(engine.JIT(Compiler))
    wat_files = ("Test_Files/001_hello.wat", "Test_Files/002_binary.wat",
                 "Test_Files/003_palindrome.wat", "Test_Files/004_factorial.wat",
                 "Test_Files/005_arrays.wat", "Test_Files/006_next_day.wat",
                 "Test_Files/007_literals.wat", "Test_Files/008_vars.wat",
                 "Test_Files/009_operators.wat", "Test_Files/010_breaks.wat")

    for wat_file_name in wat_files:
        with open(wat_file_name) as wat_file:
            print(wat_file_name)
            wat_source_code = wat_file.read()
            wasm_bytes = wat2wasm(wat_source_code)

            module = Module(store, wasm_bytes)

            import_object = make_import_object(store)

            instance = Instance(module, import_object)

            print(f'exit code {instance.exports.start()}\n------------------------\n\n')
    input()
    exit(0)


if __name__ == '__main__':
    main()
