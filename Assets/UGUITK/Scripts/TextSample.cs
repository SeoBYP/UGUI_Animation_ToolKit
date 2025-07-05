// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using Cysharp.Threading.Tasks;
// using Cysharp.Threading.Tasks.Linq;
// using LitMotion;
// using LitMotion.Extensions;
// using UGUIAnimationToolkit.Text;
//
// namespace UGUIAnimationSamples
// {
//     public class TextSample : MonoBehaviour
//     {
//         [Header("Components")] [SerializeField]
//         TextMeshProUGUI text1;
//
//         [SerializeField] TextMeshProUGUI text2;
//         [SerializeField] TextMeshProUGUI text3;
//         [SerializeField] TextMeshProUGUI text4;
//         [SerializeField] TextMeshProUGUI text5;
//         [SerializeField] TextMeshProUGUI text6;
//
//         const string ColorCode = "#FF5353";
//
//         public string FillerTextWithRichtext =
//             $"<color={ColorCode}>Grumpy wizards</color> make <b>toxic brew</b> for the evil <i>Queen</i> and <i>Jack</i>.";
//
//         public string FillerText = "Grumpy wizards make toxic brew for the evil Queen and Jack.";
//
//         public void ActiveText1()
//         {
//             text1.text = FillerTextWithRichtext;
//             if (text1.isActiveAndEnabled)
//             {
//                 text1.GetComponent<UGUIText>().Play();
//                 return;
//             }
//
//             text1.gameObject.SetActive(true);
//             text2.gameObject.SetActive(false);
//             text3.gameObject.SetActive(false);
//             text4.gameObject.SetActive(false);
//             text5.gameObject.SetActive(false);
//             text6.gameObject.SetActive(false);
//         }
//
//         public void ActiveText2()
//         {
//             text2.text = FillerTextWithRichtext;
//             if (text2.isActiveAndEnabled)
//             {
//                 text2.GetComponent<UGUIText>().Play();
//                 return;
//             }
//
//             text1.gameObject.SetActive(false);
//             text2.gameObject.SetActive(true);
//             text3.gameObject.SetActive(false);
//             text4.gameObject.SetActive(false);
//             text5.gameObject.SetActive(false);
//             text6.gameObject.SetActive(false);
//         }
//
//         public void ActiveText3()
//         {
//             text3.text = FillerText;
//             text3.ForceMeshUpdate(true);
//             if (text3.isActiveAndEnabled)
//             {
//                 text3.GetComponent<UGUIText>().Play();
//                 return;
//             }
//
//             text1.gameObject.SetActive(false);
//             text2.gameObject.SetActive(false);
//             text3.gameObject.SetActive(true);
//             text4.gameObject.SetActive(false);
//             text5.gameObject.SetActive(false);
//             text6.gameObject.SetActive(false);
//         }
//
//         public void ActiveText4()
//         {
//             text4.text = FillerText;
//             text4.ForceMeshUpdate(true);
//             if (text4.isActiveAndEnabled)
//             {
//                 text4.GetComponent<UGUIText>().Play();
//                 return;
//             }
//
//             text1.gameObject.SetActive(false);
//             text2.gameObject.SetActive(false);
//             text3.gameObject.SetActive(false);
//             text4.gameObject.SetActive(true);
//             text5.gameObject.SetActive(false);
//             text6.gameObject.SetActive(false);
//         }
//
//         public void ActiveText5()
//         {
//             text5.text = FillerText;
//             text5.ForceMeshUpdate(true);
//             if (text5.isActiveAndEnabled)
//             {
//                 text5.GetComponent<UGUIText>().Play();
//                 return;
//             }
//
//             text1.gameObject.SetActive(false);
//             text2.gameObject.SetActive(false);
//             text3.gameObject.SetActive(false);
//             text4.gameObject.SetActive(false);
//             text5.gameObject.SetActive(true);
//             text6.gameObject.SetActive(false);
//         }
//
//         public void ActiveText6()
//         {
//             text6.text = FillerText;
//             text6.ForceMeshUpdate(true);
//             if (text6.isActiveAndEnabled)
//             {
//                 text6.GetComponent<UGUIText>().Play();
//                 return;
//             }
//
//             text1.gameObject.SetActive(false);
//             text2.gameObject.SetActive(false);
//             text3.gameObject.SetActive(false);
//             text4.gameObject.SetActive(false);
//             text5.gameObject.SetActive(false);
//             text6.gameObject.SetActive(true);
//         }
//     }
// }