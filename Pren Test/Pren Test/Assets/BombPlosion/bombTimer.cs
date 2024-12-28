using UnityEngine;

public class bombTimer : MonoBehaviour
{
    [SerializeField] private ParticleSystem SparksVfx;
    [SerializeField] private Transform detonationCord;

    [SerializeField] private GameObject Xplosion;

    [SerializeField] private float timer = 3f;

    [SerializeField] private bool PlayOnStart = true;
    private bool isDetonating = false;

    private void Start()
    {
        if(PlayOnStart) isDetonating = true;
    }

    //call this function to detonate the bomb
    public void Detonate()
    {
        isDetonating = true;
    }

    private void Update()
    {
        if(isDetonating)
        {
            if(!SparksVfx.isPlaying) SparksVfx.Play();

            detonationCord.position = Vector3.Lerp(detonationCord.position, detonationCord.position - new Vector3(0, 0.2f, 0), 0.1f * Time.deltaTime);
            Invoke(nameof(Explode), timer);
        }
    }

    private object Explode()
    {
        SparksVfx.Stop();
        isDetonating = false;
        Destroy(gameObject);
        return Instantiate(Xplosion, detonationCord.position, transform.rotation);
    }
}
