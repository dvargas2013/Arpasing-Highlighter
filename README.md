# Arpasing Highlighter

- reads a reclist
- figures out what phoneme pairs are in it
- figures out which pairs are are missing.

Arpasing Highlighter has been modified slightly to try and be able to parse index.csv and OREMOComment files sorta

**The .exe in the root folder is compiled for 64bit machines** (at least I think it was XD)  
The source is Visual Studio C# .NET App  
If anyone is interested in a 32bit version or wants to modify and is having trouble setting up the project hit me up.  
My Email is on my profile.

## Reclist Syntax

It reads 1 line at a time looking for pairs  
If a line has the phonemes `ao ba ch t`,  
it will extract 3 phoneme pairs `ao ba`, `ba ch`, and `ch t`

*Note: If there's a "phoneme" with more than 6 letters, it will ignore it*

### Symbol Key:
| Symbol | Description |
|---|---|
| PP | Arpasing Phoneme (or any text really) |
| _	| Either a underscore or a space (program handles them the same) |
| , | Either a comma or a tab (yep. you guessed it. handled the same) |
| ? | Ignored random characters |

### Accepted Line Types:
| Line Syntax | Description |
|---|---|
| `PP_PP_PP` | It is most recommended that every line is just space separated phonemes |
| `?????,PP_PP_PP` | Reading index.csv (ignores everything before a comma) |
| `?????(PP_PP_PP)` | Trying to read OREMO Comment files (it tries to find parentheses) |

## If there's a line that doesn't match one of the previous types, the program ***WILL*** get confused
