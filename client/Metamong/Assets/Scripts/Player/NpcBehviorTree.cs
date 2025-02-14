using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;

public class NpcBehaviorTreeRunnder : MonoBehaviour
{
    
    public BehaviourTree myTree;
    private Context myContext;

    // Start is called before the first frame update
    void Start()
    {
        myContext = Context.CreateFromGameObject(gameObject);
        myTree = myTree.Clone();
        myTree.Bind(myContext);
    }

    // Update is called once per frame
    void Update()
    {
        if (myTree)
        {
            myTree.Update();
        }
    }
}
