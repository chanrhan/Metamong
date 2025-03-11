using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;

public class NpcBehaviorTreeRunnder : MonoBehaviour
{    
    public BehaviourTree myTree;
    private Context myContext;

    void Start()
    {
        myContext = Context.CreateFromGameObject(gameObject);
        myTree = myTree.Clone();
        myTree.Bind(myContext);
    }

    void Update()
    {
        if (myTree)
        {
            myTree.Update();
        }
    }
}
