
import sys

filepath = sys.argv[1]
try:
    with open(filepath, 'r', encoding='utf-8-sig') as f:
        print(f.read())
except Exception as e:
    print(f"Error: {e}")
    try:
        with open(filepath, 'r', encoding='latin-1') as f:
            print(f.read())
    except Exception as e2:
        print(f"Error2: {e2}")

