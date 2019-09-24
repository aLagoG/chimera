SRC := $(shell find . -type f -name "*.cs")

chimera.exe: $(SRC)
	mcs -out:chimera.exe $(SRC)

chimera_debug.exe: $(SRC)
	mcs -debug -out:chimera_debug.exe $(SRC)

clean:
	rm chimera.exe chimera.exe.mdb
