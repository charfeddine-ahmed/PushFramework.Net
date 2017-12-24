using System;
using System.Collections.Generic;
using PushFramework;

namespace PushFramework.Analytics.Contracts
{
    public class JsonReply<T>
    {
        public bool IsSynchronous
        {
            get;
            set;
        }

        public int OriginalRequestId
        {
            get;
            set;
        }

        public T TypedData
        {
            get;
            set;
        }
    }

    public class VoidJsonReply
    {
        public bool IsSynchronous
        {
            get;
            set;
        }

        public int ResponseId
        {
            get;
            set;
        }
    }


    
    public abstract class MonitorServiceAbs : PushFramework.Service
    {
    	public MonitorServiceAbs()
    	{
    		// Abstract Method Declarations:
    		this.RegisterMethod(1, "GetServerInfo");
    		this.RegisterMethod(2, "StartProfiling");
    		this.RegisterMethod(3, "StopProfiling");
    		this.RegisterMethod(4, "SubscribeToFeeds");
    		this.RegisterMethod(5, "UnsubscribeFromFeeds");
    	}
    
        public override int RoutingId
        {
            get
            {
                return 1;
            }
        }
    
        public override bool IsShared
        {
            get
            {
                return true;
            }
        }
    
        public override string Name
        {
            get
            {
                return "MonitorService";
            }
        }
    
         
        // Request Class declarations:
            
        internal class GetServerInfoRequest
        {
            public int RequestId
            {
                get;
                set;
            }
        
         }
        
                    
        internal class StartProfilingRequest
        {
            public int RequestId
            {
                get;
                set;
            }
        
         }
        
                    
        internal class StopProfilingRequest
        {
            public int RequestId
            {
                get;
                set;
            }
        
         }
        
                    
        internal class SubscribeToFeedsRequest
        {
            public int RequestId
            {
                get;
                set;
            }
        
         }
        
                    
        internal class UnsubscribeFromFeedsRequest
        {
            public int RequestId
            {
                get;
                set;
            }
        
         }
        
                
    
        // Abstract Method Declarations:
        		
        public virtual Info GetServerInfo()
        {
            throw new NotImplementedException();
        }
        
        public virtual Info GetServerInfo(Connection User)
        {
            return this.GetServerInfo();
        }
                		
        public virtual void StartProfiling()
        {
            throw new NotImplementedException();
        }
        
        public virtual void StartProfiling(Connection User)
        {
            this.StartProfiling();
        }
                		
        public virtual void StopProfiling()
        {
            throw new NotImplementedException();
        }
        
        public virtual void StopProfiling(Connection User)
        {
            this.StopProfiling();
        }
                		
        public virtual void SubscribeToFeeds()
        {
            throw new NotImplementedException();
        }
        
        public virtual void SubscribeToFeeds(Connection User)
        {
            this.SubscribeToFeeds();
        }
                		
        public virtual void UnsubscribeFromFeeds()
        {
            throw new NotImplementedException();
        }
        
        public virtual void UnsubscribeFromFeeds(Connection User)
        {
            this.UnsubscribeFromFeeds();
        }
                
        // dispatch Method
        public override void Dispatch(int methodId, object _request, Connection user)
        {
        		
            if (methodId == 1)
            {
                //
                GetServerInfoRequest request = (GetServerInfoRequest) _request;
            
                JsonResponse response = new JsonResponse();
                response.IsSynchronous = true;
                response.OriginatingRequestId = request.RequestId;
            
            
                // Now invoke:
            	try{
                response.Data = this.GetServerInfo(user);
               }// try
               catch(System.Exception ex)
            	{
            		response.IsCallSucceeded = false;
            		response.Exception = ex.Message;
            		user.SendMessage(response);
            		return;
            	}
            
            	response.IsCallSucceeded = true;
                user.SendMessage(response);
            }
                    		
            if (methodId == 2)
            {
                //
                StartProfilingRequest request = (StartProfilingRequest) _request;
            
                JsonResponse response = new JsonResponse();
                response.IsSynchronous = true;
                response.OriginatingRequestId = request.RequestId;
            
            
                // Now invoke:
            	try{
                response.Data = null;
                this.StartProfiling(user);
               }// try
               catch(System.Exception ex)
            	{
            		response.IsCallSucceeded = false;
            		response.Exception = ex.Message;
            		user.SendMessage(response);
            		return;
            	}
            
            	response.IsCallSucceeded = true;
                user.SendMessage(response);
            }
                    		
            if (methodId == 3)
            {
                //
                StopProfilingRequest request = (StopProfilingRequest) _request;
            
                JsonResponse response = new JsonResponse();
                response.IsSynchronous = true;
                response.OriginatingRequestId = request.RequestId;
            
            
                // Now invoke:
            	try{
                response.Data = null;
                this.StopProfiling(user);
               }// try
               catch(System.Exception ex)
            	{
            		response.IsCallSucceeded = false;
            		response.Exception = ex.Message;
            		user.SendMessage(response);
            		return;
            	}
            
            	response.IsCallSucceeded = true;
                user.SendMessage(response);
            }
                    		
            if (methodId == 4)
            {
                //
                SubscribeToFeedsRequest request = (SubscribeToFeedsRequest) _request;
            
                JsonResponse response = new JsonResponse();
                response.IsSynchronous = true;
                response.OriginatingRequestId = request.RequestId;
            
            
                // Now invoke:
            	try{
                response.Data = null;
                this.SubscribeToFeeds(user);
               }// try
               catch(System.Exception ex)
            	{
            		response.IsCallSucceeded = false;
            		response.Exception = ex.Message;
            		user.SendMessage(response);
            		return;
            	}
            
            	response.IsCallSucceeded = true;
                user.SendMessage(response);
            }
                    		
            if (methodId == 5)
            {
                //
                UnsubscribeFromFeedsRequest request = (UnsubscribeFromFeedsRequest) _request;
            
                JsonResponse response = new JsonResponse();
                response.IsSynchronous = true;
                response.OriginatingRequestId = request.RequestId;
            
            
                // Now invoke:
            	try{
                response.Data = null;
                this.UnsubscribeFromFeeds(user);
               }// try
               catch(System.Exception ex)
            	{
            		response.IsCallSucceeded = false;
            		response.Exception = ex.Message;
            		user.SendMessage(response);
            		return;
            	}
            
            	response.IsCallSucceeded = true;
                user.SendMessage(response);
            }
                    
        }
    
    }
    
    
}



