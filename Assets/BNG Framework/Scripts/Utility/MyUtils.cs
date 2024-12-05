#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class CopyObjectPath
{
    [MenuItem("GameObject/Copy Path", false, 0)]
    public static void CopyPath()
    {
        // Получаем выбранный объект
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            Debug.LogWarning("No GameObject selected to copy the path.");
            return;
        }

        // Получаем путь до объекта
        string path = GetFullPath(selectedObject);

        // Сохраняем путь в буфер обмена
        GUIUtility.systemCopyBuffer = path;

        Debug.Log($"Path copied to clipboard: {path}");
    }

    private static string GetFullPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform;

        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }
}
#endif