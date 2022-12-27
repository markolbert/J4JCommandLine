# Notes on the Tokenizer and Parser

There are a lot of ways to write any kind of parser. Some simply embed the syntax and semantic rules in the program logic as a bunch of if/then/else and switch statements. Some are more like "real" parsers, tokenizing the input and then applying grammatical rules to parse the tokens.

I'd never written any of the latter kind. But in researching some aspects of command line parsing I stumbled across **LL(1) grammars**... and a comment to the effect no one in their right mind would write one just to parse a command line because it'd be overkill.

So, naturally, I decided to write my parser that way :).

For more information on how LL(1) parsers work I suggest you do some web searching. Or sign up for a course on compiler design :). If you go the web route, be forewarned that there aren't a lot of good -- as in "comprehensible to someone who knows nothing about the subject" -- source documents around. Or at least I wasn't able to
find many.

But by piecing together a bunch of stuff, I think this is how an LL(1) parser works.

"LL(1)" means the parser scans tokens from **L**eft to right, and only looks **1** token ahead (the second "L" is about following/constructing "left hand" routes in a tree data structure; I'm not sure about that because I didn't use a tree-based approach).

My parser isn't, technically, an LL(1) parser because it does some pre-processing of the tokens it generates before parsing them. The main such step being to merge all tokens between a starting "quoter" token and an ending "quoter" token into a single test token.

Command lines like this:

```bash
-x -y "This is a single argument to the y switch" -z abc
```

are initially turned into the following sequence of tokens:

- KeyPrefix (i.e., the "-")
- Text (the "x")
- Separator (the " ")
- KeyPrefix (the second "-")
- Text (the "y")
- Quoter (the '"')
- Text (the "This")
- Separator (the " ")
- Text (the "is")
- Separator (the " ")
- Text (the "a")
- Separator (the " ")
- Text (the "single")
- Separator (the " ")
- Text (the "argument")
- Separator (the " ")
- Text (the "to")
- Separator (the " ")
- Text (the "the")
- Separator (the " ")
- Text (the "y")
- Separator (the " ")
- Text (the "switch")
- Quoter (the '"')
- Separator (the " ")
- KeyPrefix (the "-")
- Text (the "z")
- Separator (the " ")
- Text (the "abc")

Preprocessing turns that sequence into this:

- KeyPrefix (i.e., the "-")
- Text (the "x")
- Separator (the " ")
- KeyPrefix (the second "-")
- Text (the "y")
- Text (the "This is a single argument to the y switch")
- Separator (the " ")
- KeyPrefix (the "-")
- Text (the "z")
- Separator (the " ")
- Text (the "abc")

which is much easier to parse.
