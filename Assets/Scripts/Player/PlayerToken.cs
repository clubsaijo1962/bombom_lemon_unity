using System.Collections;
using UnityEngine;
using BomBomLemon.Board;

namespace BomBomLemon.Player
{
    public class PlayerToken : MonoBehaviour
    {
        public PlayerData Data { get; private set; }

        public void Initialize(PlayerData data)
        {
            Data = data;
            GetComponent<SpriteRenderer>().color = data.PlayerColor;
        }

        public IEnumerator MoveToTile(Tile target, float speed)
        {
            Vector3 start = transform.position;
            Vector3 end = target.transform.position;
            float elapsed = 0f;
            float duration = Vector3.Distance(start, end) / Mathf.Max(speed, 0.1f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, end, elapsed / duration);
                yield return null;
            }

            transform.position = end;
            Data.CurrentTileIndex = target.TileIndex;
        }
    }
}
