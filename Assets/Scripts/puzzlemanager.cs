using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleDrag : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;

    void OnMouseDown()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        offset = transform.position - new Vector3(mousePosition.x, mousePosition.y, 0);
        isDragging = true;
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            transform.position = (new Vector3(mousePosition.x, mousePosition.y, 0) + offset);
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }
}
