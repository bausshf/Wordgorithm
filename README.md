# Wordgorithm

A library that can be used to construct sentences with minor grammar awareness using a mix between techniques from AI and Markov chains.

# Examples

## Training

```
var trainer = new WordTrainer();
```

### Singular Input

```
trainer.Train("The quick brown fox jumps over the fence.");
trainer.Train("The quick red animal jumps over the fence.");
trainer.Train("The slow brown fox jumps over the fence.");
trainer.Train("The dumb yellow fish jumps over the fence.");
trainer.Train("The quick brown fox rolls over the hill.");
```

### Multiple Input

```
trainer.Train(new[]
{
    "The quick brown fox jumps over the fence.",
    "The quick red animal jumps over the fence.",
    "The slow brown fox jumps over the fence.",
    "The dumb yellow fish jumps over the fence.",
    "The quick brown fox rolls over the hill."
});
```

### Loading A Model

```
trainer.LoadModel("model.dat");
```

### Saving A Model

```
trainer.SaveModel("model.dat");
```

## Generating

```
var builder = trainer.CreateBuilder(minWords: 5, maxWords: 25);

var result = builder.ToString();

Console.WriteLine(result);
```

### Example Outputs

This is based on using the training data above.

```
The dumb yellow fish jumps over the hill.
Fish jumps over the hill.
Red animal jumps over the hill.
Slow brown fox jumps over the hill.
Dumb yellow fish jumps over the fence.
Slow brown fox jumps over the hill.
The dumb yellow fish jumps over the fence.
Animal jumps over the hill.
Red animal jumps over the fence.
Dumb yellow fish jumps over the hill.
Animal jumps over the fence.
The slow brown fox jumps over the fence.
Slow brown fox rolls over the hill.
Brown fox rolls over the fence.
Brown fox jumps over the hill.
Red animal jumps over the hill.
Brown fox jumps over the hill.
Brown fox rolls over the fence.
The dumb yellow fish jumps over the fence.
Quick red animal jumps over the fence.
```
