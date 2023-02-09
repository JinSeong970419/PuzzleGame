using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableCandy : MonoBehaviour
{
    private GameCandy candy;
    private IEnumerator moveCoroutine;

    private void Awake()
    {
        candy = GetComponent<GameCandy>();
    }

    public void Move(int newX, int newY, float time)
    {
        if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = MoveCoroutine(newX, newY, time);
        StartCoroutine(moveCoroutine);
    }

    // 부드럽게 웁직이기 위한 코드
    private IEnumerator MoveCoroutine(int newX, int newY, float time)
    {
        candy.X = newX;
        candy.Y = newY;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = candy.GridRef.GetWorldPosition(newX, newY);

        for (float t = 0; t <= 1*time; t+=Time.deltaTime)
        {
            candy.transform.position = Vector3.Lerp(startPosition, endPosition, t / time);
            yield return 0;
        }

        candy.transform.position = endPosition;
    }
}
