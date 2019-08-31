mcs -debug -out:chimera.exe *.cs
mono chimera.exe                            \
    test_programs/binary.chimera            \
    test_programs/factorial.chimera         \
    test_programs/hello.chimera             \
    test_programs/lists.chimera             \
    test_programs/palindrome.chimera        \
    test_programs/raw_symbols_list.chimera  \
    test_programs/variables.chimera         \
    >lexical_analysis.txt