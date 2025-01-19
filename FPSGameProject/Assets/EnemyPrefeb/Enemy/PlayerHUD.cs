using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Player Status")]
    [Tooltip("�÷��̾��� ����(�̵� �ӵ�, ü��)"), SerializeField]
    private Status status;

    [Header("HP & BloodScreen UI")]
    [Tooltip("�÷��̾��� ü�� ��µǴ� Text"), SerializeField]
    public TextMeshProUGUI textHP;
    [Tooltip("�÷��̾ ���ݹ޾��� �� ȭ�鿡 ǥ�õǴ� Image"), SerializeField]
    public Image imageredscreen;
    [SerializeField]
    private AnimationCurve curveredscreen;

    private void Awake()
    {
        status.onHPEvent.AddListener(UpdateHPHUD);
        textHP.text = "HP: " + status.MaxHP;
    }


    private void UpdateHPHUD(int previous, int current)
    {
        textHP.text = "HP: " + current;

        if (previous - current > 0)
        {
            StopCoroutine("Onredscreen");
            StartCoroutine("Onredscreen");
        }
    }
    private IEnumerator Onredscreen()
    {
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime;

            Color color = imageredscreen.color;
            color.a = Mathf.Lerp(1, 0, curveredscreen.Evaluate(percent));
            imageredscreen.color = color;

            yield return null;
        }
    }
}