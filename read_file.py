
import sys
import os

filepath = sys.argv[1]

encodings = ['utf-8', 'utf-16', 'latin-1', 'cp1252']

for enc in encodings:
    try:
        with open(filepath, 'r', encoding=enc) as f:
            content = f.read()
            print(f"--- SUCCESS with {enc} ---")
            print(content)
            sys.exit(0)
    except Exception as e:
        print(f"--- FAILED with {enc}: {e}")
