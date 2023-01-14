using System.Collections.Concurrent;

public class Check
{

    public async Task<string> RunChecks(string password)
    {
        var startTime = DateTime.Now;
        var digitLength = long.MaxValue.ToString().Length;
        var mask = "0";

        Console.WriteLine($"Starting: {startTime.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.WriteLine($"Maximum length: {digitLength}, Maximum number: {long.MaxValue}");

        var checker = new Check();

        for(int i = 1; i < digitLength; i++)
        {
            var tasks = new List<Task<string>>();

            var maxNumber = Convert.ToInt64(mask.Replace("0", "9")) + 1;

            var rangeSize = maxNumber / 10;

            long start = 0;
            var end = rangeSize;

            var ranges = new List<(long start, long end)>();

            for(int ii = 0; ii < 10; ii++)
            {
                ranges.Add((start, end));
                start+=start == 0 ? rangeSize + 1 : rangeSize;
                end+=rangeSize;
            }

            var resultBag = new ConcurrentBag<string>();

            var options = new ParallelOptions { MaxDegreeOfParallelism = 11 };
            var cancellationToken = new CancellationTokenSource();
            
            await Parallel.ForEachAsync(ranges, options, async (range, token) => {
                var result = await RunCheck(range.start, range.end, i, mask, startTime, password, cancellationToken.Token, cancellationToken);
                if(result != string.Empty)
                {
                    resultBag.Add(result);
                }
            });

            if(resultBag.Count > 0)
            {
                Console.WriteLine("");
                Console.WriteLine($"Found your password: {resultBag.First()}");
                var endTime = DateTime.Now;
                Console.WriteLine($"Finished: {endTime.ToString("yyyy-MM-dd HH:mm:ss")}");
                Console.WriteLine($"Took: {endTime.Subtract(startTime).TotalSeconds} seconds");
                return resultBag.First();
            }

            mask += "0";
        }

        return string.Empty;
    }

    public async Task <string> RunCheck(long start, long end, int digitLength, string mask, DateTime startTime, string password, CancellationToken token, CancellationTokenSource tokenSource)
    {
        var current = start;
        var rangeEnd = end;

        Console.WriteLine($"Checking {start} to {end} - Digit length: {digitLength}");

        while(current < rangeEnd)
        {
            try
            {
                token.ThrowIfCancellationRequested();
            }
            catch
            {
                Console.WriteLine($"Stopped checking {start} to {end} - Digit length: {digitLength} at {current}");
                return string.Empty;
            }

            string currentGuessS = current.ToString(mask);
  
            if(currentGuessS.Equals(password))
            {
                tokenSource.Cancel();
                return await Task.Run(() => currentGuessS);
            }
            current++;
            if(current.ToString().Length > digitLength)
            {
                return string.Empty;
            }
        }

        return string.Empty;
    }
}