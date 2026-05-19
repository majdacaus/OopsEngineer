using UnityEngine;
using UnityEngine.UI;

public class MaterialButton : MonoBehaviour
{
   private BridgeManager _bridgeManager;
   
   [SerializeField] private GameObject _beamPrefab;

   void Start()
   {
      _bridgeManager = Object.FindAnyObjectByType<BridgeManager>();
      
      Button btn =GetComponent<Button>();

      if (btn != null)
      {
         btn.onClick.AddListener(SelectThisMaterial);
      }
   }

   private void SelectThisMaterial()
   {
      if (_bridgeManager != null && _beamPrefab != null)
      {
         _bridgeManager.SetBeamPrefab(_beamPrefab);
      }
      else
      {
         Debug.LogError($"Nedostaje bridge manager ili prefab na {gameObject.name}");
      }
   }
}
