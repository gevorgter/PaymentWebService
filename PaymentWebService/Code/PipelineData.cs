using CommonDTO;
using PaymentDTO.Payment;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentWebService.Code
{
    public class PipelineSteps
    {
        [Flags]
        public enum RUNCONDITIONS { IF_FINISHED = 1, IF_NOTSUCCESSFUL = 2, IF_EXCEPTION = 4, ALL = 1 + 2 + 4};

        public Func<Task> _step;
        public string _stepName;
        public RUNCONDITIONS _runIf = RUNCONDITIONS.ALL;
        public PipelineSteps(Func<Task> step, RUNCONDITIONS runIf = 0)
        {
            _step = step;
            _stepName = step.Method.Name;
            _runIf = runIf;
        }
    }


    public abstract class PipelineData<T>
    {
        public string _requestBody;
        public int _stepNumber { get; set; } = 0;
        public bool _success { get; set; } = true;
        public List<string> _errors = null;
        public bool _pipelineFinished { get; private set; } = false;
        public int _accountId { get; set; }
        public T _rq;


        //public Dictionary<string, object> requestData = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

        public void FinishPipeline()
        {
            _pipelineFinished = true;
        }
        public void AddError(string error, bool finishPipeline = true)
        {
            _success = false;
            _pipelineFinished = finishPipeline;
            if (_errors == null)
                _errors = new List<string>();
            _errors.Add(error);
        }

        public virtual async Task ExecutePipeline()
        {
            PipelineSteps[] steps = GetPipelineSteps();
            _stepNumber = 0;
            bool exception = false;

            foreach (var step in steps)
            {
                _stepNumber++;
                bool run = true;

                if (exception)
                    run &= ((step._runIf & PipelineSteps.RUNCONDITIONS.IF_EXCEPTION) > 0);

                if (_pipelineFinished)
                    run &= ((step._runIf & PipelineSteps.RUNCONDITIONS.IF_FINISHED) > 0);
               
                if (!_success && ((step._runIf & PipelineSteps.RUNCONDITIONS.IF_NOTSUCCESSFUL) > 0))
                    run &= ((step._runIf & PipelineSteps.RUNCONDITIONS.IF_NOTSUCCESSFUL) > 0);

                if (run)
                {
                    try
                    {
                        await step._step().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        exception = true;
                        Log.Logger.Error(e, $"Pipeline threw exception on step #: {_stepNumber}");
                        AddError("Server Problem");
                    }
                }

            }
        }
        public abstract PipelineSteps[] GetPipelineSteps();
        
    }

}
