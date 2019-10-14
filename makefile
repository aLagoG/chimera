SRC := $(shell find . -type f -name "*.cs")

chimera.exe: $(SRC)
	mcs -out:chimera.exe $(SRC)

.PHONY: debug
debug: chimera_debug.exe

chimera_debug.exe: $(SRC)
	mcs -define:DEBUG -debug -out:chimera_debug.exe $(SRC)

clean:
	rm chimera*.exe chimera*.exe.mdb
