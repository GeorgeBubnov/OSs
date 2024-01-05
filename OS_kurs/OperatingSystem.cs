using OS_kurs.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OS_kurs
{
    internal class OperatingSystem
    {
        private const int QuantumOfTime = 10;
        private int MaxID = 0;
        private volatile List<ProcessOS> ProcessQueue = new List<ProcessOS>();

        public OperatingSystem()
        {
            OperationsWithProcesses();
        }
        public bool IsNotEmpty() { return ProcessQueue.Count != 0; }
        private async void OperationsWithProcesses()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    if (ProcessQueue.Count != 0)
                    {
                        SortQueque();
                        ProcessOS process = ProcessQueue.First();
                        RunOperation(process);
                    }
                }
            });
        }
        private void SortQueque()
        {
            ProcessQueue = ProcessQueue
                .OrderBy(p => p.Status)
                .ThenBy(p => p.Priority)
                .ThenBy(p => p.Time)
                .ToList();
        }
        private void RunOperation(ProcessOS process)
        {
            foreach(ProcessOS p in ProcessQueue)
            {
                if (p.Status == 'X')
                {
                    if (p.Count > 0)
                        p.Count--;
                    if(p.Count == 0)
                    {
                        p.Status = 'W';
                        if (p.Priority > -19)
                            p.Priority--;
                    }
                }
            }

            Thread.Sleep(Math.Min(QuantumOfTime, process.Time) * 10);

            if (process.Status == 'X')
                return;

            process.Count += 2;
            process.Status = 'R';

            if (QuantumOfTime > process.Time)
                ProcessQueue.Remove(process);
            else
                process.Time -= QuantumOfTime;

            if (process.Count > 10)
                process.Status = 'X';

            if (process.Time == 0)
                process.Status = 'Z';
        }
        public void AddNewProcess(int time, sbyte pri = 0)
        {
            int pid = MaxID++;
            ProcessQueue.Add(new ProcessOS(pid, time, pri));
        }
        private ProcessOS GetProcessByID(int id)
        {
            if (ProcessQueue.Count != 0)
                return ProcessQueue.Where(p => p.ID == id).First();
            return null;
        }
        public void ChangeTime(int id, int time) { GetProcessByID(id).Time = time; }
        public void ChangePriorety(int id, sbyte pri) { GetProcessByID(id).Priority = pri; }
        public string GetProcess() 
        {
            string res = "ID\tTime\tStatus\tPriorety\n";
            List<ProcessOS> plist = ProcessQueue.OrderBy(p => p.Status).ToList();
            foreach (var process in plist)
            {
                res += process.ID.ToString() + "\t";
                res += process.Time.ToString() + "\t";
                res += process.Status.ToString() + "\t";
                res += process.Priority.ToString() + "\n";
            }
            return res;
        }
        public void Remove(int id) { ProcessQueue.Remove(GetProcessByID(id)); }
    }
}
