using System.Collections;
using UnityEngine;

public class AutoDeactivate : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(Deactivate());
    }

    IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(5);
        gameObject.SetActive(false);
    }
}
