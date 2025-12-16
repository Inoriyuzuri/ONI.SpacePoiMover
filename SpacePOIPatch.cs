using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace SpacePOIMover
{
    [HarmonyPatch(typeof(ClusterMapScreen), "OnSpawn")]
    public static class ClusterMapScreen_OnSpawn_Patch
    {
        public static void Postfix(ClusterMapScreen __instance)
        {
            var existing = __instance.gameObject.GetComponent<POIMoverUI>();
            if (existing == null)
            {
                __instance.gameObject.AddComponent<POIMoverUI>();
                Debug.Log("[SpacePOIMover] POIMoverUI added");
            }
        }
    }

    public class POIMoverUI : MonoBehaviour
    {
        private GameObject panelObj;
        private GameObject contentObj;
        private TextMeshProUGUI statusText;
        private TextMeshProUGUI authorText;
        private Button moveButton;
        private Button moveAllButton;
        private Button cancelButton;
        private ClusterGridEntity lastSelectedEntity;

        private void Start()
        {
            CreatePanel();
        }

        private void CreatePanel()
        {
            panelObj = new GameObject("POIMoverPanel");
            
            var canvas = panelObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            panelObj.AddComponent<CanvasScaler>();
            panelObj.AddComponent<GraphicRaycaster>();

            contentObj = new GameObject("Content");
            contentObj.transform.SetParent(panelObj.transform, false);
            
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 1);
            contentRect.anchorMax = new Vector2(0.5f, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = new Vector2(0, 0);
            contentRect.sizeDelta = new Vector2(300, 130);

            var bg = contentObj.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.09f, 0.12f, 0.96f);

            var innerBorder = new GameObject("InnerBorder");
            innerBorder.transform.SetParent(contentObj.transform, false);
            var innerRect = innerBorder.AddComponent<RectTransform>();
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.sizeDelta = new Vector2(-4, -4);
            var innerImg = innerBorder.AddComponent<Image>();
            innerImg.color = new Color(0.18f, 0.2f, 0.25f, 0.95f);

            var topLine = new GameObject("TopLine");
            topLine.transform.SetParent(contentObj.transform, false);
            var topRect = topLine.AddComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0, 1);
            topRect.anchorMax = new Vector2(1, 1);
            topRect.pivot = new Vector2(0.5f, 1);
            topRect.sizeDelta = new Vector2(0, 2);
            topRect.anchoredPosition = Vector2.zero;
            var topImg = topLine.AddComponent<Image>();
            topImg.color = new Color(0.6f, 0.75f, 0.9f, 0.8f);

            var innerContent = new GameObject("InnerContent");
            innerContent.transform.SetParent(contentObj.transform, false);
            var innerContentRect = innerContent.AddComponent<RectTransform>();
            innerContentRect.anchorMin = Vector2.zero;
            innerContentRect.anchorMax = Vector2.one;
            innerContentRect.sizeDelta = Vector2.zero;

            var layout = innerContent.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 5;
            layout.padding = new RectOffset(14, 14, 8, 8);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(innerContent.transform, false);
            var titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "◆ POI Mover ◆";
            titleTmp.fontSize = 15;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.95f, 0.82f, 0.45f);
            titleTmp.enableAutoSizing = false;
            var titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 20;

            statusText = CreateText(innerContent.transform, "Select a POI 选择POI", 12, new Color(0.8f, 0.85f, 0.9f));

            var btnRow = new GameObject("BtnRow");
            btnRow.transform.SetParent(innerContent.transform, false);
            var btnLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
            btnLayout.spacing = 12;
            btnLayout.childAlignment = TextAnchor.MiddleCenter;
            btnLayout.childForceExpandWidth = false;
            var btnLayoutElem = btnRow.AddComponent<LayoutElement>();
            btnLayoutElem.preferredHeight = 32;

            moveButton = CreateStyledButton(btnRow.transform, "Move This 移动选中", () => OnMove(false), 
                new Color(0.15f, 0.45f, 0.3f), new Color(0.2f, 0.55f, 0.38f), 120, 28);
            moveAllButton = CreateStyledButton(btnRow.transform, "Move All 移动全部", () => OnMove(true), 
                new Color(0.25f, 0.38f, 0.52f), new Color(0.32f, 0.48f, 0.62f), 120, 28);

            cancelButton = CreateStyledButton(innerContent.transform, "取消 Cancel", OnCancel, 
                new Color(0.5f, 0.22f, 0.22f), new Color(0.6f, 0.28f, 0.28f), 120, 28);
            cancelButton.gameObject.SetActive(false);

            var authorObj = new GameObject("Author");
            authorObj.transform.SetParent(innerContent.transform, false);
            authorText = authorObj.AddComponent<TextMeshProUGUI>();
            authorText.text = "By Inoriyuzuri  QQ:523590411";
            authorText.fontSize = 14;
            authorText.fontStyle = FontStyles.Bold;
            authorText.alignment = TextAlignmentOptions.Center;
            authorText.color = new Color(0.95f, 0.75f, 0.4f);
            var authorLayout = authorObj.AddComponent<LayoutElement>();
            authorLayout.preferredHeight = 22;

            panelObj.SetActive(false);
            Debug.Log("[SpacePOIMover] Panel created!");
        }

        private Button CreateStyledButton(Transform parent, string text, UnityEngine.Events.UnityAction onClick, Color normalColor, Color hoverColor, float w, float h)
        {
            var obj = new GameObject(text + "Btn");
            obj.transform.SetParent(parent, false);

            var img = obj.AddComponent<Image>();
            img.color = normalColor;

            var highlight = new GameObject("Highlight");
            highlight.transform.SetParent(obj.transform, false);
            var hlRect = highlight.AddComponent<RectTransform>();
            hlRect.anchorMin = new Vector2(0.1f, 0);
            hlRect.anchorMax = new Vector2(0.9f, 0);
            hlRect.pivot = new Vector2(0.5f, 0);
            hlRect.sizeDelta = new Vector2(0, 2);
            hlRect.anchoredPosition = new Vector2(0, 2);
            var hlImg = highlight.AddComponent<Image>();
            hlImg.color = new Color(1f, 1f, 1f, 0.3f);

            var btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = hoverColor;
            colors.pressedColor = normalColor * 0.8f;
            colors.selectedColor = normalColor;
            btn.colors = colors;
            btn.onClick.AddListener(onClick);

            var le = obj.AddComponent<LayoutElement>();
            le.preferredWidth = w;
            le.preferredHeight = h;

            var txtObj = new GameObject("Text");
            txtObj.transform.SetParent(obj.transform, false);
            var tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 12;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            var txtRect = txtObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;

            return btn;
        }

        private TextMeshProUGUI CreateText(Transform parent, string text, int fontSize, Color color)
        {
            var obj = new GameObject("Text");
            obj.transform.SetParent(parent, false);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            var le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = fontSize + 8;
            return tmp;
        }

        private void Update()
        {
            if (panelObj == null) return;

            var clusterMap = ClusterMapScreen.Instance;
            if (clusterMap == null || !clusterMap.isActiveAndEnabled)
            {
                panelObj.SetActive(false);
                return;
            }

            ClusterGridEntity selectedEntity = null;
            var field = typeof(ClusterMapScreen).GetField("m_selectedEntity", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                selectedEntity = field.GetValue(clusterMap) as ClusterGridEntity;

            if (selectedEntity != lastSelectedEntity)
            {
                lastSelectedEntity = selectedEntity;
                OnSelectionChanged(selectedEntity);
            }

            if (POIMoverTool.IsInMoveMode)
            {
                POIMoverTool.UpdateVisuals();

                if (Input.GetMouseButtonDown(0))
                {
                    POIMoverTool.TryMove();
                    UpdateUI();
                }
                if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
                {
                    POIMoverTool.CancelMoveMode();
                    UpdateUI();
                }
            }
        }

        private void OnSelectionChanged(ClusterGridEntity entity)
        {
            if (entity == null)
            {
                panelObj.SetActive(false);
                return;
            }

            string typeName = entity.GetType().Name;
            bool isMovable = entity is HarvestablePOIClusterGridEntity ||
                entity is ArtifactPOIClusterGridEntity ||
                typeName.Contains("POI") ||
                typeName.Contains("Inventory");

            if (entity is AsteroidGridEntity || entity is ClusterMapMeteorShowerVisualizer)
                isMovable = false;

            if (!isMovable)
            {
                panelObj.SetActive(false);
                return;
            }

            POIMoverTool.CurrentEntity = entity;
            panelObj.SetActive(true);
            UpdateUI();
        }

        private void OnMove(bool moveAll)
        {
            POIMoverTool.MoveAllEntities = moveAll;
            POIMoverTool.StartMoveMode();
            UpdateUI();
        }

        private void OnCancel()
        {
            POIMoverTool.CancelMoveMode();
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (statusText == null) return;

            var entity = POIMoverTool.CurrentEntity;
            if (POIMoverTool.IsInMoveMode)
            {
                string mode = POIMoverTool.MoveAllEntities ? "全部" : "选中";
                statusText.text = $"<color=#FFD54F>点击目标位置 ({mode})</color>";
                moveButton?.gameObject.SetActive(false);
                moveAllButton?.gameObject.SetActive(false);
                cancelButton?.gameObject.SetActive(true);
                authorText?.gameObject.SetActive(false);
            }
            else
            {
                if (entity != null)
                    statusText.text = $"<size=14>{entity.Name}</size>  <color=#888>({entity.Location.R}, {entity.Location.Q})</color>";
                moveButton?.gameObject.SetActive(true);
                moveAllButton?.gameObject.SetActive(true);
                cancelButton?.gameObject.SetActive(false);
                authorText?.gameObject.SetActive(true);
            }
        }
    }

    public static class POIMoverTool
    {
        public static ClusterGridEntity CurrentEntity { get; set; }
        public static bool IsInMoveMode { get; private set; }
        public static bool MoveAllEntities { get; set; }

        private static GameObject lineObj;
        private static Image lineImage;
        private static RectTransform lineRect;
        private static GameObject targetCircle;
        private static Image targetImage;
        private static GameObject glowCircle;

        public static void StartMoveMode()
        {
            if (CurrentEntity == null) return;
            IsInMoveMode = true;
            CreateVisuals();
        }

        public static void CancelMoveMode()
        {
            IsInMoveMode = false;
            HideVisuals();
        }

        private static void CreateVisuals()
        {
            var clusterMap = ClusterMapScreen.Instance;
            if (clusterMap == null) return;

            var canvas = clusterMap.GetComponentInChildren<Canvas>();
            if (canvas == null) return;

            if (lineObj == null)
            {
                lineObj = new GameObject("POIMoverLine");
                lineObj.transform.SetParent(canvas.transform, false);
                
                lineImage = lineObj.AddComponent<Image>();
                lineImage.color = new Color(0.3f, 0.9f, 0.5f, 0.85f);
                lineRect = lineObj.GetComponent<RectTransform>();
                lineRect.pivot = new Vector2(0, 0.5f);
                lineRect.anchorMin = new Vector2(0.5f, 0.5f);
                lineRect.anchorMax = new Vector2(0.5f, 0.5f);

                var glow = new GameObject("LineGlow");
                glow.transform.SetParent(lineObj.transform, false);
                var glowImg = glow.AddComponent<Image>();
                glowImg.color = new Color(0.3f, 1f, 0.5f, 0.25f);
                var glowRect = glow.GetComponent<RectTransform>();
                glowRect.anchorMin = Vector2.zero;
                glowRect.anchorMax = Vector2.one;
                glowRect.sizeDelta = new Vector2(0, 8);
            }
            lineObj.SetActive(true);

            if (targetCircle == null)
            {
                glowCircle = new GameObject("TargetGlow");
                glowCircle.transform.SetParent(canvas.transform, false);
                var glowImg = glowCircle.AddComponent<Image>();
                glowImg.color = new Color(0.3f, 1f, 0.5f, 0.2f);
                glowCircle.GetComponent<RectTransform>().sizeDelta = new Vector2(70, 70);

                targetCircle = new GameObject("POIMoverTarget");
                targetCircle.transform.SetParent(canvas.transform, false);
                targetImage = targetCircle.AddComponent<Image>();
                targetImage.color = new Color(0.3f, 0.95f, 0.5f, 0.5f);
                
                var outline = targetCircle.AddComponent<Outline>();
                outline.effectColor = new Color(0.2f, 1f, 0.4f, 0.9f);
                outline.effectDistance = new Vector2(2, 2);

                targetCircle.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);

                var innerCircle = new GameObject("InnerCircle");
                innerCircle.transform.SetParent(targetCircle.transform, false);
                var innerImg = innerCircle.AddComponent<Image>();
                innerImg.color = new Color(1f, 1f, 1f, 0.3f);
                var innerRect = innerCircle.GetComponent<RectTransform>();
                innerRect.anchorMin = new Vector2(0.5f, 0.5f);
                innerRect.anchorMax = new Vector2(0.5f, 0.5f);
                innerRect.sizeDelta = new Vector2(15, 15);
            }
            targetCircle.SetActive(true);
            glowCircle.SetActive(true);
        }

        private static void HideVisuals()
        {
            if (lineObj != null) lineObj.SetActive(false);
            if (targetCircle != null) targetCircle.SetActive(false);
            if (glowCircle != null) glowCircle.SetActive(false);
        }

        public static void UpdateVisuals()
        {
            if (!IsInMoveMode || lineObj == null || targetCircle == null) return;

            var clusterMap = ClusterMapScreen.Instance;
            if (clusterMap == null) return;

            var canvas = clusterMap.GetComponentInChildren<Canvas>();
            if (canvas == null) return;

            Vector2 startScreen = GetEntityScreenPos(CurrentEntity);
            if (startScreen == Vector2.zero) return;

            var targetHex = GetHexAtMouse();
            Vector2 endScreen = targetHex.HasValue ? GetHexScreenPos(targetHex.Value) : (Vector2)Input.mousePosition;
            bool isValid = targetHex.HasValue && POIMoveManager.IsValidDestination(targetHex.Value, CurrentEntity);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(), startScreen, null, out Vector2 startLocal);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(), endScreen, null, out Vector2 endLocal);

            Vector2 dir = endLocal - startLocal;
            float dist = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            lineRect.anchoredPosition = startLocal;
            lineRect.sizeDelta = new Vector2(dist, 4f);
            lineRect.localRotation = Quaternion.Euler(0, 0, angle);

            targetCircle.GetComponent<RectTransform>().anchoredPosition = endLocal;
            glowCircle.GetComponent<RectTransform>().anchoredPosition = endLocal;

            Color validLineColor = new Color(0.3f, 0.95f, 0.5f, 0.85f);
            Color validCircleColor = new Color(0.3f, 0.95f, 0.5f, 0.5f);
            Color validGlowColor = new Color(0.3f, 1f, 0.5f, 0.2f);
            Color validOutlineColor = new Color(0.2f, 1f, 0.4f, 0.9f);

            Color invalidLineColor = new Color(0.95f, 0.35f, 0.3f, 0.85f);
            Color invalidCircleColor = new Color(0.95f, 0.35f, 0.3f, 0.5f);
            Color invalidGlowColor = new Color(1f, 0.3f, 0.3f, 0.2f);
            Color invalidOutlineColor = new Color(1f, 0.25f, 0.2f, 0.9f);

            if (isValid)
            {
                lineImage.color = validLineColor;
                targetImage.color = validCircleColor;
                glowCircle.GetComponent<Image>().color = validGlowColor;
                targetCircle.GetComponent<Outline>().effectColor = validOutlineColor;
                lineObj.transform.GetChild(0).GetComponent<Image>().color = new Color(0.3f, 1f, 0.5f, 0.25f);
            }
            else
            {
                lineImage.color = invalidLineColor;
                targetImage.color = invalidCircleColor;
                glowCircle.GetComponent<Image>().color = invalidGlowColor;
                targetCircle.GetComponent<Outline>().effectColor = invalidOutlineColor;
                lineObj.transform.GetChild(0).GetComponent<Image>().color = new Color(1f, 0.3f, 0.3f, 0.25f);
            }
        }

        public static void TryMove()
        {
            var targetHex = GetHexAtMouse();
            if (!targetHex.HasValue) return;
            if (!POIMoveManager.IsValidDestination(targetHex.Value, CurrentEntity)) return;

            var oldLoc = CurrentEntity.Location;
            var newLoc = targetHex.Value;

            if (MoveAllEntities && ClusterGrid.Instance != null)
            {
                var toMove = new List<ClusterGridEntity>();
                foreach (var e in ClusterGrid.Instance.GetEntitiesOnCell(oldLoc))
                {
                    if (!(e is AsteroidGridEntity) && !(e is ClusterMapMeteorShowerVisualizer))
                        toMove.Add(e);
                }
                foreach (var e in toMove)
                {
                    e.Location = newLoc;
                    Debug.Log($"[SpacePOIMover] Moved {e.Name} to {newLoc}");
                }
            }
            else
            {
                CurrentEntity.Location = newLoc;
                Debug.Log($"[SpacePOIMover] Moved {CurrentEntity.Name} to {newLoc}");
            }

            CancelMoveMode();
        }

        private static Vector2 GetEntityScreenPos(ClusterGridEntity entity)
        {
            var clusterMap = ClusterMapScreen.Instance;
            if (clusterMap == null) return Vector2.zero;
            foreach (var hex in clusterMap.GetComponentsInChildren<ClusterMapHex>())
            {
                if (hex.location.Equals(entity.Location))
                {
                    var rt = hex.GetComponent<RectTransform>();
                    Vector3[] corners = new Vector3[4];
                    rt.GetWorldCorners(corners);
                    return RectTransformUtility.WorldToScreenPoint(null, (corners[0] + corners[2]) / 2f);
                }
            }
            return Vector2.zero;
        }

        private static Vector2 GetHexScreenPos(AxialI loc)
        {
            var clusterMap = ClusterMapScreen.Instance;
            if (clusterMap == null) return Input.mousePosition;
            foreach (var hex in clusterMap.GetComponentsInChildren<ClusterMapHex>())
            {
                if (hex.location.Equals(loc))
                {
                    var rt = hex.GetComponent<RectTransform>();
                    Vector3[] corners = new Vector3[4];
                    rt.GetWorldCorners(corners);
                    return RectTransformUtility.WorldToScreenPoint(null, (corners[0] + corners[2]) / 2f);
                }
            }
            return Input.mousePosition;
        }

        private static AxialI? GetHexAtMouse()
        {
            var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            pointer.position = Input.mousePosition;
            var results = new List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer, results);
            foreach (var r in results)
            {
                var hex = r.gameObject.GetComponent<ClusterMapHex>() ?? r.gameObject.GetComponentInParent<ClusterMapHex>();
                if (hex != null) return hex.location;
            }
            return null;
        }
    }
}
