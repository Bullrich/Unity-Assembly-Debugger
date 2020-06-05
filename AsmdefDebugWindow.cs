using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace AssemblyDebugger
{
    public class AsmdefDebugWindow : EditorWindow
    {
        private static string reportJson;
        private static CompilationReport report;
        private bool logEnabled;

        [MenuItem("Window/Assemblies Debugger")]
        private static void Init()
        {
            var window = (AsmdefDebugWindow) GetWindow(typeof(AsmdefDebugWindow), false, "Assembly Debugger");
            window.Show();
        }

#if !IGNORE_ASMDEF_DEBUG
        [DidReloadScripts]
#endif
        private static void OnReload()
        {
            reportJson = EditorPrefs.GetString(AsmdefDebug.CompilationReportEditorPref);
        }

        private static CompilationReport GenerateReport(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonUtility.FromJson<CompilationReport>(json);
        }

        private void OnEnable()
        {
            logEnabled = EditorPrefs.GetBool(AsmdefDebug.LogEnabledPref, true);
        }

        private void OnGUI()
        {
            if (report == null)
            {
                report = GenerateReport(reportJson);
            }

            if (report == null)
            {
                EditorGUILayout.HelpBox(
                    "No compilation report found. Modify a script to trigger a recompilation", MessageType.Warning);
                return;
            }

            GUILayout.Label("Post Compilation Report", EditorStyles.boldLabel);

            var date = DateTime.FromBinary(report.reloadEventTimes);
            var totalTimeSeconds = report.compilationTotalTime + AsmdefDebug.AssemblyReloadTime.TotalSeconds;

            EditorGUILayout.TextField("Compilation Report", $"{totalTimeSeconds:F2} seconds", EditorStyles.boldLabel);
            var orderedCompilations = report.assemblyCompilations.OrderBy(x => x.compilationTime).Reverse();
            foreach (var assCom in orderedCompilations)
            {
                EditorGUILayout.TextField(assCom.assemblyName, $"{assCom.compilationTime:0.00}s", EditorStyles.label);
            }

            EditorGUILayout.FloatField("Assembly Reload Time", (float) AsmdefDebug.AssemblyReloadTime.TotalSeconds,
                EditorStyles.boldLabel);


            GUILayout.Label("Print compilation time after reload", EditorStyles.boldLabel);
            var enableLog = EditorGUILayout.Toggle("Use Debug.Log", logEnabled);

            if (logEnabled != enableLog)
            {
                EditorPrefs.SetBool(AsmdefDebug.LogEnabledPref, enableLog);
                logEnabled = enableLog;
            }
        }
    }
}