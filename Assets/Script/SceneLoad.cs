using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // 在Inspector面板中赋值，或者直接使用场景名
    public string targetSceneName = "1";
    public void LoadTargetScene()
    {
        // 使用SceneManager加载目标场景
        SceneManager.LoadScene(targetSceneName);
    }
}