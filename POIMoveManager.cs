using System.Collections.Generic;
using UnityEngine;

namespace SpacePOIMover
{
    public static class POIMoveManager
    {
        public static bool IsValidDestination(AxialI location, ClusterGridEntity entityToMove)
        {
            if (ClusterGrid.Instance == null) return false;
            if (!ClusterGrid.Instance.IsValidCell(location)) return false;

            string movingType = entityToMove.GetType().Name;
            bool isMovingInventory = movingType.Contains("Inventory") || movingType == "StarmapHexCellInventoryVisuals";

            var entities = ClusterGrid.Instance.GetEntitiesOnCell(location);
            foreach (var entity in entities)
            {
                // 不能移动到小行星上
                if (entity is AsteroidGridEntity) return false;
                
                // 跳过自己
                if (entity == entityToMove) continue;
                
                string existingType = entity.GetType().Name;
                bool isExistingInventory = existingType.Contains("Inventory") || existingType == "StarmapHexCellInventoryVisuals";
                bool isExistingPOI = entity is HarvestablePOIClusterGridEntity || entity is ArtifactPOIClusterGridEntity;
                
                // POI和可收获资源可以重叠
                if (isMovingInventory && isExistingPOI) continue;
                if (!isMovingInventory && isExistingInventory) continue;
                
                // 同类型不能重叠
                if (entity is HarvestablePOIClusterGridEntity && entityToMove is HarvestablePOIClusterGridEntity) return false;
                if (entity is ArtifactPOIClusterGridEntity && entityToMove is ArtifactPOIClusterGridEntity) return false;
                if (isExistingInventory && isMovingInventory) return false;
            }
            return true;
        }
    }
}
