using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public interface ITextEmbeddable
{
    double GetCosinSimilarity(string org, string target);
}