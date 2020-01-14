using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace SiteAccelerator
{
    /// <summary>
    /// 提供windows服务控制
    /// </summary>
    static class WindowsService
    {
        private static readonly Command command = Command.None;
        private static readonly string binPath;
        private static readonly Mode mode;
        private static readonly Mutex mutex;

        /// <summary>
        /// 获取服务名
        /// </summary>
        public static string Name { get; }

        /// <summary>
        /// 获取应用是否为第一个运行实例
        /// </summary>
        public static bool? IsFirstInstance { get; }

        /// <summary>
        /// windows服务控制
        /// </summary>
        static WindowsService()
        {
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (Enum.TryParse(arg, ignoreCase: true, out command))
                {
                    break;
                }
            }

            var process = Process.GetCurrentProcess();
            binPath = process.MainModule.FileName;
            Name = Path.GetFileNameWithoutExtension(binPath);

            if (command != Command.None)
            {
                mode = Mode.CmdTool;
            }
            else
            {
                mode = process.SessionId == 0 ? Mode.Service : Mode.Desktop;
            }

            if (mode != Mode.CmdTool)
            {
                try
                {
                    mutex = new Mutex(true, @$"Global\{Name}", out var mutexCreatedNew);
                    IsFirstInstance = mutexCreatedNew;
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// 如果是以服务运行
        /// 则应用进程工作目录和ContentRoot为进程文件的目录
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IHostBuilder UseBinPathEnvironment(this IHostBuilder hostBuilder)
        {
            if (mode == Mode.Service)
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(binPath);
                hostBuilder.UseContentRoot(Environment.CurrentDirectory);
            }

            return hostBuilder;
        }


        /// <summary>
        /// 应用命令控制服务
        /// start安装和启动服务
        /// stop停止和删除服务
        /// </summary>
        /// <returns></returns>
        public static bool UseCtrlCommand()
        {
            switch (command)
            {
                case Command.Start:
                    StopService();
                    DeleteService();
                    CreateService();
                    StartService();
                    break;

                case Command.Stop:
                    StopService();
                    DeleteService();
                    break;
            }

            return mode == Mode.CmdTool;
        }

        /// <summary>
        /// 安装服务
        /// </summary>
        private static void CreateService()
        {
            var args = Arguments.Create("create", Name).And("binPath=", binPath).And("start=", "auto");
            RunWaitSC(args);
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        private static void StartService()
        {
            RunWaitSC(Arguments.Create("start", Name));
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        private static void StopService()
        {
            var serviceProcess = Process.GetProcessesByName(Name)
                .Where(item => item.SessionId == 0)
                .FirstOrDefault();

            RunWaitSC(Arguments.Create("stop", Name));

            if (serviceProcess != null)
            {
                serviceProcess.WaitForExit(30 * 1000);
            }
        }

        /// <summary>
        /// 删除服务
        /// </summary>
        private static void DeleteService()
        {
            RunWaitSC(Arguments.Create("delete", Name));
        }

        /// <summary>
        /// 启动sc
        /// </summary>
        /// <param name="args"></param>
        private static void RunWaitSC(Arguments args)
        {
            var info = new ProcessStartInfo
            {
                CreateNoWindow = true,
                FileName = "sc.exe",
                Arguments = args.ToString(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            Process.Start(info).WaitForExit();
        }

        /// <summary>
        /// 运行模式
        /// </summary>
        private enum Mode
        {
            /// <summary>
            /// 服务
            /// </summary>
            Service,

            /// <summary>
            /// 桌面
            /// </summary>
            Desktop,

            /// <summary>
            /// 工具
            /// </summary>
            CmdTool
        }

        /// <summary>
        /// 命令
        /// </summary>
        private enum Command
        {
            /// <summary>
            /// 无命令
            /// </summary>
            None,

            /// <summary>
            /// 启动
            /// </summary>
            Start,

            /// <summary>
            /// 停止
            /// </summary>
            Stop,
        }

        /// <summary>
        /// 表示命令行
        /// </summary>
        private class Arguments
        {
            private readonly Dictionary<string, string> arguments = new Dictionary<string, string>();

            /// <summary>
            /// 命令行
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            private Arguments(string key, object value)
            {
                this.And(key, value);
            }

            /// <summary>
            /// 创建命令
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            /// <returns></returns>
            public static Arguments Create(string key, object value)
            {
                return new Arguments(key, value);
            }

            /// <summary>
            /// 添加命令
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            /// <returns></returns>
            public Arguments And(string key, object value)
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                if (key.IndexOf(' ') >= 0)
                {
                    throw new ArgumentException(nameof(key));
                }
                this.arguments[key.Trim()] = GetValue(value);
                return this;
            }

            /// <summary>
            /// 转换值
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            private static string GetValue(object value)
            {
                if (value == null)
                {
                    return null;
                }

                var val = value.ToString().Trim();
                return val.IndexOf(' ') >= 0 && val.StartsWith('"') == false ? $"\"{val}\"" : val;
            }

            public override string ToString()
            {
                var args = this.arguments.Select(item => $"{item.Key} {item.Value}");
                return string.Join(' ', args);
            }
        }
    }
}
