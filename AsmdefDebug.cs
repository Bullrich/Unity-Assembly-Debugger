using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDebugger;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;


// Based on https://gist.github.com/karljj1/9c6cce803096b5cd4511cf0819ff517b
[InitializeOnLoad]
public class AsmdefDebug
{
    internal const string CompilationReportEditorPref = "CompilationReportKey";
    internal const string LogEnabledPref = "AsmdefDebugLogKey";

    private static readonly int ScriptAssembliesPathLen = "Library/ScriptAssemblies/".Length;
    private static readonly CompilationReport CompilationReport = new CompilationReport();
    private static readonly Dictionary<string, DateTime> StartTimes = new Dictionary<string, DateTime>();
    internal static TimeSpan AssemblyReloadTime { get; private set; }

    private static double compilationTotalTime;

#if !IGNORE_ASMDEF_DEBUG
    static AsmdefDebug()
    {
        CompilationPipeline.assemblyCompilationStarted += CompilationPipelineOnAssemblyCompilationStarted;
        CompilationPipeline.assemblyCompilationFinished += CompilationPipelineOnAssemblyCompilationFinished;
        AssemblyReloadEvents.beforeAssemblyReload += AssemblyReloadEventsOnBeforeAssemblyReload;
        AssemblyReloadEvents.afterAssemblyReload += AssemblyReloadEventsOnAfterAssemblyReload;
    }
#endif

    private static void CompilationPipelineOnAssemblyCompilationStarted(string assembly)
    {
        StartTimes[assembly] = DateTime.UtcNow;
    }

    private static void CompilationPipelineOnAssemblyCompilationFinished(string assembly, CompilerMessage[] arg2)
    {
        var timeSpan = DateTime.UtcNow - StartTimes[assembly];
        compilationTotalTime += timeSpan.TotalMilliseconds;
        var assemblyTime = (timeSpan.TotalMilliseconds / 1000);
        var assemblyName = assembly.Substring(ScriptAssembliesPathLen, assembly.Length - ScriptAssembliesPathLen);
        CompilationReport.assemblyCompilations.Add(new AssemblyCompilation(assemblyName, assemblyTime));
    }

    private static void AssemblyReloadEventsOnBeforeAssemblyReload()
    {
        var totalCompilationTimeSeconds = compilationTotalTime / 1000f;
        CompilationReport.compilationTotalTime = totalCompilationTimeSeconds;
        CompilationReport.reloadEventTimes = DateTime.UtcNow.ToBinary();
        EditorPrefs.SetString(CompilationReportEditorPref, JsonUtility.ToJson(CompilationReport));
    }

    private static void AssemblyReloadEventsOnAfterAssemblyReload()
    {
        var reportJson = EditorPrefs.GetString(CompilationReportEditorPref);
        if (string.IsNullOrEmpty(reportJson))
        {
            return;
        }

        var report = JsonUtility.FromJson<CompilationReport>(reportJson);

        var date = DateTime.FromBinary(report.reloadEventTimes);
        AssemblyReloadTime = DateTime.UtcNow - date;

        if (!EditorPrefs.GetBool(LogEnabledPref, true))
        {
            return;
        } 

        var totalTimeSeconds = report.compilationTotalTime + AssemblyReloadTime.TotalSeconds;
        if (report.assemblyCompilations != null && report.assemblyCompilations.Any())
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Compilation Report: {totalTimeSeconds:F2} seconds");
            var orderedCompilations = report.assemblyCompilations.OrderBy(x => x.compilationTime).Reverse();
            foreach (var assCom in orderedCompilations)
            {
                builder.AppendFormat("{0:0.00}s {1}\n", assCom.compilationTime, assCom.assemblyName);
            }

            builder.AppendFormat("Assembly Reload Time: {0}\n", AssemblyReloadTime.TotalSeconds);

            Debug.Log(builder.ToString());
        }
    }
}