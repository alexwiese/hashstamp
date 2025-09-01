# Performance Report

## Benchmark Results: Original vs Optimized

| Method | Original | Optimized | Change |
|--------|----------|-----------|---------|
| CompileTimeHashAccess (Median) | 0.361 μs | 0.311 μs | **13.8% faster** |
| RuntimeHashAccess (Median) | 1.443 μs | 1.749 μs | 21.2% slower |
| CountAllMethods (Median) | 6.101 μs | 5.365 μs | **12.1% faster** |

## Summary

The string optimization changes show mixed results:
- **CompileTimeHashAccess**: 13.8% performance improvement 
- **CountAllMethods**: 12.1% performance improvement
- **RuntimeHashAccess**: 21.2% performance regression

The optimizations primarily benefit operations that involve hash generation and string processing during source generation, while runtime hash lookups show some performance regression.