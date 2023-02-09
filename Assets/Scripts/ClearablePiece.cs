using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearablePiece : MonoBehaviour
{
    public AnimationClip clearAnimation;

    private bool isBeingCleared = false;
    public bool IsBeingCleared
    {
        get { return isBeingCleared; }
    }

    protected GameCandy piece;

    private void Awake()
    {
        piece = GetComponent<GameCandy>();
    }

    public virtual void Clear()
    {
        isBeingCleared = true;
        StartCoroutine(ClearCorutine());
    }

    private IEnumerator ClearCorutine()
    {
        Animator animator = GetComponent<Animator>();

        if (animator)
        {
            animator.Play(clearAnimation.name);

            yield return new WaitForSeconds(clearAnimation.length);

            Destroy(gameObject);
        }
    }

}
