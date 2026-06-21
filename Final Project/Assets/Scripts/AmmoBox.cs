using System.Collections;
using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider col)
    {
        StartCoroutine(Active());
    }
    public IEnumerator Active()
    {
        gameObject.SetActive(false);
        yield return new WaitForSeconds(10);
        gameObject.SetActive(true);
    }
}
