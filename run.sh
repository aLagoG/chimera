file="$@"
ext="${file#*.}"
base="${file%%.*}"
filename=$(basename -- "$base")

set -e
echo "--- MAKE ---"
make debug
echo "--- COMPILE ---"
mono --debug chimera_debug.exe "$file" > "$base".ast
echo "--- ILASM ---"
ilasm /debug "$base".il -out:./"$filename".exe
echo "--- RUN ---"
mono --debug "$filename".exe
