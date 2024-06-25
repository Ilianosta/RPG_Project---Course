using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TargetDamage : Life
{
    public GameObject targetPoint;
    public Material blue;
    public Material red;
    public MeshRenderer mesh;
    public GameObject text;
    public Transform model;
    public bool inDamage;
    public GameObject player;

    private void Awake()
    {
        mesh = GetComponentInChildren<MeshRenderer>();
        model = mesh.transform;
    }

    public override void GetHit(int damage)
    {
        if (inDamage) return;
        base.GetHit(damage);
        inDamage = true;

        mesh.material = red;

        model.DOShakePosition(1f, 1, 10, 90, false, true, ShakeRandomnessMode.Full).OnComplete(() => model.localPosition = Vector3.zero);
        GameObject textInstance = Instantiate(text, UIManager.instance.transform);

        Text textComp = textInstance.GetComponent<Text>();
        textComp.text = damage.ToString();
        textInstance.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);

        RectTransform textRectTransf = textInstance.GetComponent<RectTransform>();
        float y = textRectTransf.position.y;

        textRectTransf.DOMoveY(y + 250f, 1f).OnComplete(() => Destroy(textInstance));
        textComp.DOFade(0, 1);

        Time.timeScale = 0;
        Sequence time = DOTween.Sequence();
        time.AppendInterval(.2f).OnComplete(() => Time.timeScale = 1).SetUpdate(true);

        Sequence s = DOTween.Sequence();
        s.AppendInterval(1).OnComplete(() =>
        {
            inDamage = false;
            if (currentLife > 0)
            {
                mesh.material = blue;
            }
            else
            {
                this.enabled = false;
                Destroy(gameObject, .2f);
            }
        });
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Life>().GetHit(25);
        }
    }

    private void OnDestroy()
    {
        if (player)
        {
            player.GetComponent<PlayerMotion>().NoTarget();
            player = null;
        }
    }
}
