using AlphaMDHealth.Model;
using AlphaMDHealth.ServiceBusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMDHealth.ServiceLayer
{
    [Route("api/QuestionnaireService")]
    [ApiAuthorize]
    public class QuestionnaireController : BaseController
    {
        /// <summary>
        /// Get questionnaires
        /// </summary>
        /// <param name="questionnaireID">questionnaire id</param>
        /// <param name="organisationID">organisation Id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <param name="languageID">language id</param>
        /// <returns>Questionnaires data</returns>
        [Route("GetQuestionnairesAsync")]
        [HttpGet]
        public async Task<IActionResult> GetQuestionnairesAsync(long questionnaireID, long organisationID, long permissionAtLevelID, long recordCount, byte languageID)
        {
            return HttpActionResult(await new QuestionnaireServiceBusinessLayer(HttpContext).GetQuestionnairesAsync(questionnaireID, organisationID, languageID, permissionAtLevelID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Saves questionnaire
        /// </summary>
        /// <param name="languageID">Language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="organisationID">organisation id</param>
        /// <param name="questionnaireData">reference object which holds questionnaire data</param>
        /// <returns>Operation status</returns>
        [Route("SaveQuestionnaireAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveQuestionnaireAsync(byte languageID, long permissionAtLevelID, long organisationID, [FromBody] QuestionnaireDTO questionnaireData)
        {
            return HttpActionResult(await new QuestionnaireServiceBusinessLayer(HttpContext).SaveQuestionnaireAsync(languageID, permissionAtLevelID, organisationID, questionnaireData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Saves questionnaire status
        /// </summary>
        /// <param name="languageID">Language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="questionnaireID">The questionaire to Publish or unpulish</param>
        /// <param name="isPublished">Publish= True, Unpublish= False</param>
        /// <returns>Operation status</returns>
        [Route("PublishQuestionnaireAsync")]
        [HttpPost]
        public async Task<IActionResult> PublishQuestionnaireAsync(byte languageID, long permissionAtLevelID, long questionnaireID, bool isPublished)
        {
            return HttpActionResult(await new QuestionnaireServiceBusinessLayer(HttpContext).PublishQuestionnaireAsync(languageID, permissionAtLevelID, questionnaireID, isPublished).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Gets Questionnaire question data
        /// </summary>
        /// <param name="languageID">Users language ID</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="recordCount">Record count for retriving list or single data</param>
        /// <param name="questionnaireID">Questionnaire ID</param>
        /// <param name="questionID">Question ID</param>
        /// <returns>Questionnaire question(s)</returns>
        [Route("GetQuestionnaireQuestionsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetQuestionnaireQuestionsAsync(byte languageID, long permissionAtLevelID, long recordCount, long questionnaireID, long questionID)
        {
            return HttpActionResult(await new QuestionnaireServiceBusinessLayer(HttpContext).GetQuestionnaireQuestionsAsync(languageID, permissionAtLevelID, recordCount, questionnaireID, questionID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Saves Questionnaire question data
        /// </summary>
        /// <param name="languageID">Users language ID</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="organisationID">organisation id</param>
        /// <param name="questionData">reference object which holds question data</param>
        /// <returns>Operation Status</returns>
        [Route("SaveQuestionnaireQuestionAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveQuestionnaireQuestionAsync(byte languageID, long permissionAtLevelID, long organisationID, [FromBody] QuestionnaireDTO questionData)
        {
            return HttpActionResult(await new QuestionnaireServiceBusinessLayer(HttpContext).SaveQuestionnaireQuestionAsync(permissionAtLevelID, organisationID, questionData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get Subscale data of the questionnaire
        /// </summary>
        /// <param name="languageID">Language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="questionnaireID">questionnaire id</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <param name="subscaleID">subscale id to fetch</param>
        /// <param name="subscaleRangeID">subscale range id to fetch</param>
        /// <returns>Questionnaire Subscale Data</returns>
        [Route("GetQuestionnaireSubscaleAsync")]
        [HttpGet]
        public async Task<IActionResult> GetQuestionnaireSubscaleAsync(byte languageID, long permissionAtLevelID, long questionnaireID, long recordCount, long subscaleID, long subscaleRangeID)
        {
            return HttpActionResult(await new QuestionnaireServiceBusinessLayer(HttpContext).GetQuestionnaireSubscaleAsync(languageID, permissionAtLevelID, questionnaireID, recordCount, subscaleID, subscaleRangeID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Saves Subscale data
        /// </summary>
        /// <param name="languageID">Language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="questionnaireID">Selected Questionnaire ID</param>
        /// <param name="questionnaireSubscale">Selected subscale value</param>
        /// <returns>Operation status</returns>
        [Route("SaveQuestionnaireSubscaleAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveQuestionnaireSubscaleAsync(byte languageID, long permissionAtLevelID, long questionnaireID, string scoreTypeID)
        {
            return HttpActionResult(await new QuestionnaireServiceBusinessLayer(HttpContext).SaveQuestionnaireSubscaleAsync(languageID, permissionAtLevelID, questionnaireID, scoreTypeID).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Saves Subscale ranges
        /// </summary>
        /// <param name="languageID">Language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="organisationID">organisation id</param>
        /// <param name="questionnaireData">reference object which holds Subscale ranges</param>
        /// <returns>Operation status</returns>
        [Route("SaveQuestionnaireSubscaleRangesAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveQuestionnaireSubscaleRangesAsync(byte languageID, long permissionAtLevelID, long organisationID, [FromBody] QuestionnaireDTO questionnaireData)
        {
            return HttpActionResult(await new QuestionnaireServiceBusinessLayer(HttpContext).SaveQuestionnaireSubscaleRangesAsync(languageID, permissionAtLevelID, organisationID, questionnaireData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Saves Patient Questionnaire Results
        /// </summary>
        /// <param name="languageID">Language ID</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="questionnaireData">reference object which holds patient questionnaire results</param>
        /// <returns>ErrorCode and New Server GUID in case of duplicate GUID</returns>
        [Route("SavePatientQuestionnaireResultsAsync")]
        [HttpPost]
        public async Task<IActionResult> SavePatientQuestionnaireResultsAsync(byte languageID, long permissionAtLevelID, [FromBody] QuestionnaireDTO questionnaireData)
        {
            return HttpActionResult(await new QuestionnaireServiceBusinessLayer(HttpContext).SavePatientQuestionnaireResultsAsync(permissionAtLevelID, questionnaireData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Saves Questionnaire linked question data
        /// </summary>
        /// <param name="languageID">Users language ID</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="organisationID">organisation id</param>
        /// <param name="questionData">reference object which holds question data</param>
        /// <returns>Operation Status</returns>
        [Route("SaveQuestionConditionsAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveQuestionConditionsAsync(byte languageID, long permissionAtLevelID, long organisationID, [FromBody] QuestionnaireDTO questionData)
        {
            return HttpActionResult(await new QuestionnaireServiceBusinessLayer(HttpContext).SaveQuestionConditionsAsync(permissionAtLevelID, organisationID, questionData).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get questionnaires condition async
        /// </summary>
        /// <param name="questionnaireID">questionnaire id</param>
        /// <param name="questionID">question ID</param>
        /// <param name="organisationID">organisation Id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <param name="languageID">language id</param>
        /// <returns>Questionnaires data</returns>
        [Route("GetQuestionConditionsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetQuestionConditionsAsync(long questionnaireID, long questionID,long organisationID, long permissionAtLevelID, long recordCount, byte languageID)
        {
            return HttpActionResult(await new QuestionnaireServiceBusinessLayer(HttpContext).GetQuestionConditionsAsync(questionnaireID, questionID, organisationID, languageID, permissionAtLevelID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Get questionnaires question Score async
        /// </summary>
        /// <param name="questionnaireID">questionnaire id</param>
        /// <param name="questionID">question ID</param>
        /// <param name="organisationID">organisation Id</param>
        /// <param name="permissionAtLevelID">level at which permission is required</param>
        /// <param name="recordCount">number of record count to fetch</param>
        /// <param name="languageID">language id</param>
        /// <returns>Questionnaires score data</returns>
        [Route("GetQuestionScoreAsync")]
        [HttpGet]
        public async Task<IActionResult> GetQuestionScoreAsync(long questionnaireID, long questionID, long organisationID, long permissionAtLevelID, long recordCount, byte languageID)
        {
            return HttpActionResult(await new QuestionnaireServiceBusinessLayer(HttpContext).GetQuestionScoreAsync(questionnaireID, questionID, organisationID, languageID, permissionAtLevelID, recordCount).ConfigureAwait(false), languageID);
        }

        /// <summary>
        /// Saves Questionnaire  question score data
        /// </summary>
        /// <param name="languageID">Users language ID</param>
        /// <param name="permissionAtLevelID">Level at which permission is required</param>
        /// <param name="organisationID">organisation id</param>
        /// <param name="questionData">reference object which holds question data</param>
        /// <returns>Operation Status</returns>
        [Route("SaveQuestionScoreAsync")]
        [HttpPost]
        public async Task<IActionResult> SaveQuestionScoreAsync(byte languageID, long permissionAtLevelID, long organisationID, [FromBody] QuestionnaireDTO questionData)
        {
            return HttpActionResult(await new QuestionnaireServiceBusinessLayer(HttpContext).SaveQuestionScoreAsync(permissionAtLevelID, organisationID, questionData).ConfigureAwait(false), languageID);
        }
    }
}
