#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class CopyObjectPath
{
    [MenuItem("GameObject/Copy Path", false, 0)]
    public static void CopyPath()
    {
        // �������� ��������� ������
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            Debug.LogWarning("No GameObject selected to copy the path.");
            return;
        }

        // �������� ���� �� �������
        string path = GetFullPath(selectedObject);

        // ��������� ���� � ����� ������
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