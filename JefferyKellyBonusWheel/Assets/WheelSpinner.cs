using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WheelSpinner : MonoBehaviour
{
    [SerializeField]
    List<PrizeEntry> prizes;
    Timer spinTimer;

    Timer lingerTimer;
    float lingerTime = 1.0f;
    int startPosition = -1;

    [SerializeField]
    GameObject panel;
    bool hasBeenSpun = false;
    // Start is called before the first frame update
    void Awake()
    {
        spinTimer = new Timer(0);
        lingerTimer = new Timer(lingerTime);
        
    }

    // Update is called once per frame
    void Update()
    {
        TimerManager.Instance.Update(Time.deltaTime);
    }

    public void SpinWheel()
    {
        if (!hasBeenSpun)
        {
            int prize = DeterminePrize();
            SpinTowardsPrize(prize);

            spinTimer.OnComplete.AddListener(() =>
            {
                lingerTimer.OnComplete.RemoveAllListeners();
                lingerTimer.OnComplete.AddListener(() =>
                {
                    SwapPanels(prize);
                });
                lingerTimer.Start();
                
            });
            hasBeenSpun = true;
        }
    }

    public void SwapPanels(int prize)
    {
        if (transform.parent.gameObject.activeSelf)
        {
            panel.SetActive(true);
            Image duplicateImage = Instantiate(prizes[prize].prizeOnWheel).GetComponent<Image>();
            duplicateImage.transform.SetParent(panel.transform);
            duplicateImage.rectTransform.localPosition = Vector3.zero;
            duplicateImage.rectTransform.localScale = new Vector3(3, 3, 1);
            //duplicateImage.rectTransform.sizeDelta = new Vector2(270, 270);
            duplicateImage.rectTransform.rotation = Quaternion.identity;
            transform.parent.gameObject.SetActive(false);
        } else
        {
            Destroy(panel.transform.GetChild(1).gameObject);
            panel.SetActive(false);
            transform.parent.gameObject.SetActive(true);
            hasBeenSpun = false;
        }
    }

    void SpinTowardsPrize(int prizeNumber)
    {
        int numFullSpins = Random.Range(2, 11);
        int endSpace = 2 * prizeNumber;
       
        spinTimer = new Timer(0.03f, numFullSpins * 16 +  endSpace - startPosition - 1);
        spinTimer.OnTick.AddListener(() =>
        {
            transform.Rotate(Vector3.forward, 22.5f);
        });

        spinTimer.OnComplete.AddListener(() =>
        {
            startPosition = endSpace;
        });

        spinTimer.Start();
    }

    IEnumerator TestSpins()
    {
        for (int i = 0; i < prizes.Count; i++)
        {
            SpinTowardsPrize(i);
            yield return new WaitForSecondsRealtime(10.0f);
        }
        yield return null;
    }
    
    void RunTestSpins(int numSpins)
    {
        int[] results = new int[prizes.Count];

        for (int i = 0; i < prizes.Count; i++)
        {
            results[i] = 0;
        }

        for (int i = 0; i < numSpins; i++)
        {
            int index = DeterminePrize();
            results[index]++;
        }

        Debug.Log(string.Format("Here are the results after {0} test spins", numSpins));
        for (int i = 0; i < prizes.Count; i++)
        {
            Debug.Log(string.Format("Prize #{0} ({1}) won {2} times", i, prizes[i].prizeName, results[i]));
        }
    }

    int DeterminePrize()
    {
        float totalChance = 0.0f;
        float randy = Random.value;
        for (int j = 0; j < prizes.Count; j++)
        {
            totalChance += prizes[j].winChance;

            if (randy <= totalChance)
            {
                return j;
            }
        }

        return -1;
    }
}

[System.Serializable]
public struct PrizeEntry
{
    public string prizeName;
    public float winChance;
    public GameObject prizeOnWheel;
}
