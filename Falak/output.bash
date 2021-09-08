#!/bin/bash
#Running all test files 

if [ -f "output.txt" ]; then
    rm "output.txt"
fi
for f in Test_Files/*.falak
do
    mono falak.exe $f >> "output.txt"
done