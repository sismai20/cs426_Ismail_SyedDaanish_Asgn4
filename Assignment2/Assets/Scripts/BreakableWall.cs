using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    public Color allowedColor;

    void Start()
    {
        // tint the wall
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", mat.GetColor("_BaseColor") * allowedColor);
                else if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", mat.GetColor("_Color") * allowedColor);
            }
        }
    }

	private void OnCollisionEnter(Collision collision)
	{
		if (!collision.gameObject.CompareTag("Bullet")) return;

		var bulletRenderer = collision.gameObject.GetComponent<Renderer>();
		if (bulletRenderer == null) return;

		Color bulletColor = bulletRenderer.material.color;
		if (ColorsMatch(bulletColor, allowedColor))
		{
			Destroy(collision.gameObject);
			Destroy(gameObject);
		}
	}

    private bool ColorsMatch(Color a, Color b)
    {
        return Vector4.Distance((Vector4)a, (Vector4)b) < 0.3f;
    }
}