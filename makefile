SRC := $(shell find . -type f -name "*.cs" -maxdepth 1)

chimera.exe: $(SRC) ChimeraLib.dll
	mcs -out:chimera.exe $(SRC)

.PHONY: debug
debug: chimera_debug.exe ChimeraLib.dll

chimera_debug.exe: $(SRC)
	mcs -define:DEBUG -debug -out:chimera_debug.exe $(SRC)

ChimeraLib.dll: ChimeraLib.cs
	mcs -t:library ChimeraLib.cs -out:ChimeraLib.dll

clean:
	rm -f *.exe
	rm -f *.exe.mdb
	rm -f **/*.ast
	rm -f **/*.il
	rm -f *.dll
