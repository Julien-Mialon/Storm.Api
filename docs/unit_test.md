# Unit Test Backlog

> This project has **no tests yet**. Each unchecked box below is one test to add.
> Scope is **unit tests only** — no real database, HTTP, or Redis. Anything that requires those is listed in [§14 Out of scope](#14-out-of-scope-integration-not-unit).

## Conventions

- Test project: `tests/Storm.Api.Tests` — xUnit + FluentAssertions + NSubstitute (to be created).
- Source generator tests: `tests/Storm.Api.SourceGenerators.Tests` using `Microsoft.CodeAnalysis.CSharp.SourceGenerators.Testing`.
- Always mock `TimeProvider` for time-dependent code (per `CLAUDE.md`).
- Test naming: `MethodName_Scenario_ExpectedBehavior`.
- Group tests per source file in a `{ClassName}Tests` fixture.

---

## 1. CQRS Core

### `src/Storm.Api/CQRS/BaseAction.cs`

- [x] `Execute_WithNullParameter_ThrowsBadRequest` — default `ValidateParameter` returns false for null
- [x] `Execute_WithValidParameter_CallsValidateThenPrepareThenAction_InOrder`
- [x] `Execute_WithCustomValidationFalse_ThrowsBadRequest`
- [x] `Execute_WithCustomValidationTrue_ExecutesAction`
- [x] `Execute_ReturnsActionResult`
- [x] `Execute_PrepareParameterCalledOnlyAfterSuccessfulValidation`
- [x] `Execute_ActionNotCalled_WhenValidationFails`

### `src/Storm.Api/CQRS/BaseAuthenticatedAction.cs`

- [x] `Execute_WithNullParameter_ThrowsBadRequest`
- [x] `Execute_WhenAuthenticatorReturnsNull_ThrowsUnauthorized`
- [x] `Execute_CallsValidateThenPrepareThenAuthenticateThenAuthorizeThenAction_InOrder`
- [x] `Execute_AuthorizeCalledWithResolvedAccount`
- [x] `Execute_ActionCalledWithBothParameterAndAccount`
- [x] `Execute_DefaultAuthorizeIsNoOp`
- [x] `Execute_CustomAuthorizeCanThrow_PreventsActionFromRunning`
- [x] `Execute_ReturnsActionResult_OnHappyPath`

### `src/Storm.Api/CQRS/Unit.cs` *(path: `src/Storm.Api/CQRS/Unit.cs` or `src/Storm.Api/Unit.cs`)*

- [x] `Default_IsNotNull`
- [x] `Default_ReturnsSameInstanceAcrossCalls`
- [x] `Constructor_IsPrivate_PreventsDirectInstantiation`

---

## 2. Exceptions

### `src/Storm.Api/CQRS/Exceptions/DomainException.cs`

- [x] `Constructor_ErrorCodeAndMessage_SetsProperties`
- [x] `Constructor_WithInner_PreservesInnerExceptionChain`
- [x] `Message_IsForwardedToBaseException`
- [x] `EmptyErrorCode_IsAllowed`

### `src/Storm.Api/CQRS/Exceptions/DomainHttpCodeException.cs`

- [x] `Constructor_CodeOnly_ConvertsHttpStatusToInt`
- [x] `Constructor_CodeOnly_UsesCodeToStringAsErrorCode`
- [x] `Constructor_CodeOnly_UsesEmptyStringAsErrorMessage`
- [x] `Constructor_CodeAndMessage_SetsErrorMessage`
- [x] `Constructor_CodeAndErrorCodeAndMessage_SetsBoth`
- [x] `Constructor_WithInner_PreservesInnerException`
- [x] `Code_BadRequest_Returns400`
- [x] `Code_Unauthorized_Returns401`
- [x] `Code_Forbidden_Returns403`
- [x] `Code_NotFound_Returns404`
- [x] `Code_InternalServerError_Returns500`

### `src/Storm.Api/CQRS/Exceptions/DomainDatabaseException.cs`

- [x] `DefaultConstructor_ReturnsServiceUnavailable503`
- [x] `Constructor_WithMessage_KeepsDefault503`
- [x] `PrimaryUnavailable_Factory_SetsPrimaryUnavailableErrorCode`
- [x] `PrimaryUnavailable_WithReason_UsesReasonAsErrorMessage`
- [x] `PrimaryUnavailable_WithoutReason_UsesDefaultMessage`
- [x] `Constructor_CustomHttpCode_OverridesDefault503`
- [x] `Constructor_WithInner_PreservesInnerException`

### `src/Storm.Api/CQRS/Extensions/ExceptionExtensions.cs`

> `BadRequestIfNull`, `UnauthorizedIfNull`, `ForbiddenIfNull`, `NotFoundIfNull`, `DomainHttpCodeExceptionIfNull`, `DomainExceptionIfNull` and their `If(True|False)` / `Task<T>` / struct variants. Pattern test per method family.

- [x] `BadRequestIfNull_WithNull_Throws400`
- [x] `BadRequestIfNull_WithValue_ReturnsValue`
- [x] `BadRequestIfNull_CustomErrorCodeAndMessage_PopulatesBothOnException`
- [x] `BadRequestIfNull_Struct_UnwrapsNullableToNonNullableValue`
- [x] `BadRequestIfNull_TaskOverload_AwaitsAndValidates`
- [x] `UnauthorizedIfNull_WithNull_Throws401`
- [x] `ForbiddenIfNull_WithNull_Throws403`
- [x] `NotFoundIfNull_WithNull_Throws404`
- [x] `DomainHttpCodeExceptionIfNull_CustomStatus_ThrowsWithCustomCode`
- [x] `DomainExceptionIfNull_WithNull_ThrowsDomainException`
- [x] `BadRequestIfTrue_WithTrue_Throws`
- [x] `BadRequestIfTrue_WithFalse_ReturnsFalse`
- [x] `BadRequestIfFalse_WithFalse_Throws`
- [x] `BadRequestIfFalse_WithTrue_ReturnsTrue`
- [x] `UnauthorizedIfTrue_TaskOverload_AwaitsAndThrows`
- [x] `ForbiddenIfFalse_WithCustomCodeAndMessage_PopulatesBoth`
- [x] `NotFoundIfTrue_WithFalse_ReturnsFalse`
- [x] `DomainExceptionIfTrue_WithTrue_ThrowsDomainException`
- [x] `DomainExceptionIfFalse_WithFalse_ThrowsDomainException`

---

## 3. Controllers & DTOs

### `src/Storm.Api/Controllers/BaseController.cs`

- [x] `WrapForError_ActionReturnsResponse_KeepsSuccessTrue`
- [x] `WrapForError_ActionReturnsNonResponse_WrapsInResponseT`
- [x] `WrapForError_DomainHttpCodeException_ReturnsStatusCodeWithErrorBody`
- [x] `WrapForError_DomainHttpCodeException_NullErrorCode_UsesGenericFallback`
- [x] `WrapForError_DomainException_Returns200WithErrorBody`
- [x] `WrapForError_GenericException_Logs_Critical`
- [x] `WrapForError_GenericException_Rethrows_InDevEnvironment`
- [x] `WrapForError_GenericException_Returns500_InProduction`
- [x] `WrapForErrorRaw_PassesIActionResultThrough`
- [x] `InternalWrapForError_ReturnsActionResultT`
- [x] `Action_Unit_ExecutesResolvedActionAndWraps`
- [x] `Action_TypedOutput_WrapsInResponseT`
- [x] `FileAction_ByteResult_ReturnsFileWithContentTypeAndFilename`
- [x] `FileAction_ByteResult_NoFilename_OmitsFilename`
- [x] `FileAction_StreamResult_ReturnsFileStreamResult`
- [x] `FileAction_UnsupportedFormat_ThrowsInvalidOperationException`

### `src/Storm.Api.Dtos/Response.cs`

- [x] `Response_DefaultValues_IsSuccessFalse_NullErrorFields`
- [x] `Response_Serialize_SystemTextJson_UsesSnakeCasePropertyNames`
- [x] `Response_Serialize_NewtonsoftJson_UsesSnakeCasePropertyNames`
- [x] `ResponseT_SerializeWithData_IncludesDataField`
- [x] `ResponseT_SerializeNullData_EmitsNull`
- [x] `Response_DeserializeRoundtrip_PreservesAllFields`

### `src/Storm.Api.Dtos/PaginatedResponse.cs`

- [x] `PaginatedResponse_Defaults_AllNumericFieldsZero_DataNull`
- [x] `PaginatedResponse_Serialize_IncludesPageCountTotalCountData`
- [x] `PaginatedResponse_EmptyArray_SerializesAsEmptyArray`
- [x] `PaginatedResponse_InheritsIsSuccessAndErrorFields`

### `src/Storm.Api.Dtos/LoginResponse.cs`

- [x] `LoginResponse_Default_AccessTokenIsEmptyString`
- [x] `LoginResponse_Serialize_IncludesAllFields`
- [x] `LoginResponse_SerializeNullRefreshToken_EmitsNull`

### `src/Storm.Api/CQRS/Domains/Results/ApiFileResult.cs`

- [x] `Create_FromBytes_ReturnsFileByteResult`
- [x] `Create_FromStream_ReturnsFileStreamResult`
- [x] `Create_DefaultFileNameIsFile`
- [x] `Create_ContentTypeStoredOnInit`
- [x] `FileByteResult_IsRawDataTrue_IsStreamDataFalse`
- [x] `FileByteResult_AsRawData_ReturnsOriginalBytes`
- [x] `FileByteResult_AsStreamData_ThrowsInvalidOperation`
- [x] `FileStreamResult_IsStreamDataTrue_IsRawDataFalse`
- [x] `FileStreamResult_AsStreamData_ReturnsOriginalStream`
- [x] `FileStreamResult_AsRawData_ThrowsInvalidOperation`

### `src/Storm.Api/CQRS/Domains/Parameters/FileParameter.cs`

- [x] `IsEmpty_NullInputStream_ReturnsTrue`
- [x] `IsEmpty_LengthZero_ReturnsTrue`
- [x] `IsEmpty_StreamAndNonZeroLength_ReturnsFalse`
- [x] `LoadFrom_NullFile_ReturnsSelfUntouched`
- [x] `LoadFrom_EmptyFile_ReturnsSelfUntouched`
- [x] `LoadFrom_CopyStreamFalse_UsesOpenedReadStream`
- [x] `LoadFrom_CopyStreamTrue_CreatesMemoryStreamAtPositionZero`
- [x] `LoadFrom_CopiesAllPropertiesFromFormFile`
- [x] `LoadFrom_ReturnsThis_SupportsChaining`
- [x] `Dispose_DisposesInputStream`
- [x] `Dispose_NullInputStream_IsNoOp`
- [x] `DisposeAsync_DisposesInputStream`

### `src/Storm.Api/CQRS/Domains/Parameters/PaginationParameter.cs`

- [x] `Defaults_PageAndCountZero`
- [x] `Setters_MutateValues`

### `src/Storm.Api/CQRS/Domains/Results/FileContentType.cs`

- [x] `Constants_MatchExpectedMimeTypes` *(single data-driven test covering all constants)*

---

## 4. Source Generators

### `src/Storm.Api.SourceGenerators/ActionMethods/ActionMethodCodeGenerator.cs`

- [x] `Generator_PartialClass_WithWithActionAttribute_EmitsSource`
- [x] `Generator_NonPartialClass_DoesNotEmit`
- [x] `Generator_ClassWithoutWithActionAttribute_DoesNotEmit`
- [x] `Generator_NonClassNode_Skipped`
- [x] `Generator_MultipleClassesInFile_EmitsOnePerEligibleClass`
- [ ] `Generator_TransformationThrows_ReportsDiagnostic`

### `src/Storm.Api.SourceGenerators/ActionMethods/ContextTransformer.cs`

- [x] `CreateContext_ResolvesAllRequiredTypes`
- [ ] `CreateContext_ReturnsNull_WhenARequiredTypeIsMissing`
- [x] `CreateContext_SkipsMethodsWithoutWithActionAttribute`
- [x] `CreateContext_DetectsActionType_Unit`
- [x] `CreateContext_DetectsActionType_Response`
- [x] `CreateContext_DetectsActionType_ApiFileResult`
- [x] `CreateContext_DetectsActionType_RegularT`
- [x] `CreateContext_ExtractsActionTypeFromWithActionGeneric`
- [x] `CreateContext_ParameterWithMapTo_MapsExplicitly`
- [x] `CreateContext_ParameterWithoutMapTo_AutoMapsByNameAndType`
- [x] `CreateContext_MultipleParametersSameType_NameMatchPrioritized`
- [ ] `CreateContext_UnmatchedActionProperty_Skipped`
- [x] `CreateContext_MethodWithNoParameters_UsesDefaultConstructor`
- [ ] `CreateContext_ActionNotImplementingIAction_MethodSkipped`
- [ ] `CreateContext_ExceptionDuringTransformation_CapturedAsDiagnostic`

### `src/Storm.Api.SourceGenerators/ActionMethods/CodeGenerator.cs`

- [x] `Generate_OutputStartsWithAutoGeneratedHeader`
- [x] `Generate_EmitsNullableEnableDirective`
- [x] `Generate_IncludesRequiredUsings`
- [x] `Generate_EmitsNamespaceDeclaration_WhenNamespaceIsNotGlobal`
- [x] `Generate_OmitsNamespaceDeclaration_ForGlobalNamespace`
- [x] `Generate_EmitsPartialClassWrapper`
- [x] `Generate_UnitAction_ReturnsTaskActionResultOfResponse`
- [x] `Generate_UnitAction_WrapsWithInternalWrapForError`
- [x] `Generate_ResponseAction_ReturnsTaskActionResultOfResponse`
- [x] `Generate_RegularAction_ReturnsTaskActionResultOfResponseT`
- [x] `Generate_RegularAction_SetsDataAndIsSuccessTrue`
- [x] `Generate_FileAction_ReturnsTaskIActionResult_CallsFileAction`
- [x] `Generate_ZeroParameters_UsesUnitDefaultOrNew`
- [x] `Generate_MappedParameters_EmitsObjectInitializer`
- [x] `Generate_MethodIsAsync`
- [x] `Generate_UsesServicesExecuteAction`

### `src/Storm.Api.SourceGenerators/ActionMethods/ActionMethodConstants.cs`

- [x] `WithAction_TargetsMethod_HasOneGenericParameter`
- [x] `MapTo_TargetsParameter_HasFieldNameStringInConstructor`

### `src/Storm.Api.SourceGenerators/Bases/BaseCodeGenerator.cs`

- [x] `Initialize_NoAttributes_RegistersNoPostInitOutput`
- [x] `Initialize_WithAttributes_RegistersPostInitOutput`
- [x] `AttributeClassGeneration_IncludesAutoGeneratedHeader`
- [x] `AttributeClassGeneration_EmitsAttributeUsageTargets`
- [x] `AttributeClassGeneration_EmitsInheritedAndAllowMultipleFlags`
- [x] `AttributeClassGeneration_AppliesGeneratedCodeAttribute`
- [x] `AttributeClassGeneration_Generics_AppendsToClassName`
- [x] `AttributeClassGeneration_ClassConstraint_Emitted`
- [x] `AttributeClassGeneration_StructConstraint_Emitted`
- [x] `AttributeClassGeneration_NewConstraint_Emitted`
- [x] `AttributeClassGeneration_TypeConstraints_Emitted`
- [x] `AttributeClassGeneration_CombinedConstraints_EmittedInCorrectOrder`
- [x] `AttributeClassGeneration_PropertiesWithInConstructorFalse_EmitsGetSetOnly`
- [x] `AttributeClassGeneration_PropertiesWithInConstructorTrue_EmitsConstructor`
- [x] `AttributeClassGeneration_ConstructorArgumentName_IsCamelCased`
- [x] `AttributeClassGeneration_FileName_UsesMetadataName`

### `src/Storm.Api.SourceGenerators/Bases/AttributeDefinition.cs`

- [x] `MetadataName_CombinesNamespaceAndName`
- [x] `FullName_EmittedAsExpected`
- [x] `Generics_StoredInOrder`
- [x] `Properties_StoredInOrder`
- [x] `AttributePropertyDefinition_InConstructorTrue_SetsCamelCaseConstructorArgName`
- [x] `AttributePropertyDefinition_InConstructorFalse_ConstructorArgNameIsNull`
- [x] `GenericConstraintDefinition_IsClass_Serializes_class`
- [x] `GenericConstraintDefinition_IsStruct_Serializes_struct`
- [x] `GenericConstraintDefinition_HasNew_Serializes_newParen`
- [x] `GenericConstraintDefinition_TypeConstraints_SerializedCommaSeparated`

---

## 5. Extensions

### `src/Storm.Api/Extensions/ByteExtensions.cs`

- [x] `ToHexString_EmptyArray_ReturnsEmptyString`
- [x] `ToHexString_AllZeroBytes_ReturnsZeros`
- [x] `ToHexString_AllFFBytes_ReturnsFFs`
- [x] `ToHexString_MixedBytes_ReturnsCorrectHex`
- [x] `ToHexString_OutputIsLowercase_Or_Uppercase` *(pin current behavior)*

### `src/Storm.Api/Extensions/CollectionExtensions.cs`

- [x] `ConvertAll_Enumerable_NullSource_Throws`
- [x] `ConvertAll_Enumerable_AppliesMapperToEach`
- [x] `ConvertAll_Collection_PreallocatesListSize`
- [x] `ConvertAll_TaskOverload_AwaitsThenMaps`
- [x] `None_EmptyEnumerable_ReturnsTrue`
- [x] `None_NonEmptyEnumerable_ReturnsFalse`
- [x] `None_WithPredicate_NoMatch_ReturnsTrue`
- [x] `None_WithPredicate_AnyMatch_ReturnsFalse`
- [x] `AddRange_HashSet_AddsAllItems_SkippingDuplicates`
- [x] `ToSafeDictionary_NullSource_ReturnsEmptyDictionary`
- [x] `ToSafeDictionary_BuildsDictionaryFromSelectors`
- [x] `ToListOrDefault_Null_ReturnsEmptyList`
- [x] `ToListOrDefault_NonNull_ReturnsListWithItems`
- [x] `IndexOfMin_EmptyList_Throws`
- [x] `IndexOfMin_ReturnsFirstIndexOnTies`
- [x] `IndexOfMin_UsesCustomComparer`
- [x] `IndexOfMax_EmptyList_Throws`
- [x] `IndexOfMax_ReturnsFirstIndexOnTies`
- [x] `IndexOfMax_UsesCustomComparer`

### `src/Storm.Api/Extensions/ConfigurationExtensions.cs`

- [x] `OnSection_SectionExists_InvokesAction`
- [x] `OnSection_SectionMissing_SkipsAction`
- [x] `WithSection_SectionExists_ReturnsActionResult`
- [x] `WithSection_SectionMissing_ReturnsNull`

### `src/Storm.Api/Extensions/DateTimeExtensions.cs`

- [x] `ToTimestamp_NonUtcDateTime_ThrowsInvalidOperation`
- [x] `ToTimestamp_Epoch_ReturnsZero`
- [x] `ToTimestamp_KnownDate_ReturnsExpectedUnixSeconds`
- [x] `FromTimestamp_Zero_ReturnsEpoch`
- [x] `FromTimestamp_Negative_ReturnsEpoch`
- [x] `FromTimestamp_KnownValue_ReturnsExpectedDate`
- [x] `IsPast_PastDate_ReturnsTrue`
- [x] `IsPast_FutureDate_ReturnsFalse`
- [x] `IsFuture_FutureDate_ReturnsTrue`
- [x] `IsToday_SameCalendarDay_ReturnsTrue`
- [x] `IsToday_DifferentDay_ReturnsFalse`
- [x] `IsThisWeek_SameIsoWeek_ReturnsTrue`
- [x] `IsThisWeek_DifferentWeek_ReturnsFalse`
- [x] `AsMonday_Sunday_ShiftsBackSixDays`
- [x] `AsMonday_Monday_ReturnsSameDate`
- [x] `AsMonday_Tuesday_ShiftsBackOneDay`
- [x] `AsMonday_Wednesday_ShiftsBackTwoDays`
- [x] `AsMonday_Thursday_ShiftsBackThreeDays`
- [x] `AsMonday_Friday_ShiftsBackFourDays`
- [x] `AsMonday_Saturday_ShiftsBackFiveDays`
- [x] `TodayIsInRange_NullRange_ReturnsFalse`
- [x] `TodayIsInRange_WithinRange_ReturnsTrue`
- [x] `TodayIsInRange_BoundaryInclusive_BehavesAsImplemented`

### `src/Storm.Api/Extensions/EnvironmentExtensions.cs`

- [x] `SimpleEnvironmentName_NoDelimiter_ReturnsOriginal`
- [x] `SimpleEnvironmentName_WithDashSuffix_ReturnsPrefix`
- [x] `SimpleEnvironmentName_LeadingDelimiter_ReturnsOriginal`

### `src/Storm.Api/Extensions/ExceptionExtensions.cs` *(`IsDatabaseException`)*

- [x] `IsDatabaseException_SqlException_ReturnsTrue`
- [x] `IsDatabaseException_InvalidOperationWithDbMessage_ReturnsTrue`
- [x] `IsDatabaseException_InvalidOperationGeneric_ReturnsFalse`
- [x] `IsDatabaseException_NullRefWithOrmLiteStackTrace_ReturnsTrue`
- [x] `IsDatabaseException_AggregateContainingDbException_ReturnsTrue`
- [x] `IsDatabaseException_UnrelatedException_ReturnsFalse`

### `src/Storm.Api/Extensions/FormFileExtensions.cs`

- [x] `ToFileParameter_NullFile_ReturnsNull`
- [x] `ToFileParameter_EmptyFile_ReturnsNull`
- [x] `ToFileParameter_CopyStreamFalse_UsesDirectStream`
- [x] `ToFileParameter_CopyStreamTrue_CopiesToMemoryStream`
- [x] `ToFileParameter_PropagatesAllMetadataFields`

### `src/Storm.Api/Extensions/HttpRequestExtensions.cs`

- [x] `TryGetHeader_HeaderPresent_ReturnsTrimmedValue`
- [x] `TryGetHeader_HeaderMissing_ReturnsFalse`
- [x] `TryGetHeader_WhitespaceOnly_TreatedAsMissing`
- [x] `TryGetQueryParameter_Present_ReturnsTrimmedValue`
- [x] `TryGetQueryParameter_Missing_ReturnsFalse`
- [x] `TryGetHeaderOrQueryParameter_HeaderAndQueryBothPresent_HeaderWins`
- [x] `TryGetHeaderOrQueryParameter_OnlyQuery_ReturnsQueryValue`
- [x] `TryGetHeaderOrQueryParameter_Neither_ReturnsFalse`
- [x] `RequestCulture_FeatureMissing_ReturnsInvariantCulture`
- [x] `RequestCulture_FeaturePresent_ReturnsResolvedCulture`

### `src/Storm.Api/Extensions/NullableExtensions.cs`

- [x] `Let_Struct_Null_ActionNotInvoked`
- [x] `Let_Struct_NonNull_ActionInvokedWithValue`
- [x] `Let_Class_Null_ActionNotInvoked`
- [x] `Let_Class_NonNull_ActionInvokedWithValue`
- [x] `LetIf_ConditionFalse_ActionNotInvoked` *(**expected to fail today** — documents bug: `condition` parameter is ignored in current implementation)*
- [x] `LetParseEnum_ValidString_InvokesWithParsedEnum`
- [x] `LetParseEnum_InvalidString_ActionNotInvoked`
- [x] `LetParseEnum_Null_ActionNotInvoked`

### `src/Storm.Api/Extensions/NumberExtensions.cs`

- [x] `IsPositive_Zero_ReturnsTrue`
- [x] `IsPositive_Negative_ReturnsFalse`
- [x] `IsStrictlyPositive_Zero_ReturnsFalse`
- [x] `IsStrictlyPositive_Positive_ReturnsTrue`
- [x] `IsNegative_Zero_ReturnsTrue`
- [x] `IsStrictlyNegative_Zero_ReturnsFalse`
- [x] `IsNull_Null_ReturnsTrue` *(nullable overloads)*
- [x] `IsNotNull_Value_ReturnsTrue`
- [x] `FloatingPoint_NaN_BehavesAsImplemented` *(pin current behavior for float/double NaN)*
- [x] `AllNumericTypes_DataDriven_BehaveIdentically` *(xUnit `Theory` across int/long/float/double/decimal)*

### `src/Storm.Api/Extensions/ObjectExtensions.cs`

- [x] `IsNull_Null_ReturnsTrue`
- [x] `IsNull_NonNull_ReturnsFalse`
- [x] `IsNotNull_Null_ReturnsFalse`
- [x] `IsNotNull_NonNull_ReturnsTrue`

### `src/Storm.Api/Extensions/ParseExtensions.cs`

- [x] `ToGuid_ValidString_ReturnsGuid`
- [x] `ToGuid_InvalidString_ReturnsNull`
- [x] `ToGuid_EmptyOrWhitespace_ReturnsNull`

### `src/Storm.Api/Extensions/PasswordExtensions.cs`

- [x] `AsSha256_KnownInput_MatchesKnownHash`
- [x] `AsSha256_EmptyString_MatchesKnownEmptyHash`
- [x] `AsSha256_SameInput_Deterministic`
- [x] `AsSha256_DifferentInputs_ProduceDifferentHashes`

### `src/Storm.Api/Extensions/RandomExtensions.cs`

- [x] `RandomAlphaString_LengthZero_ReturnsEmptyString`
- [x] `RandomAlphaString_Length_ReturnsRequestedLength`
- [x] `RandomAlphaString_ContainsOnlyAlphaChars`
- [x] `RandomAlphaDigitsString_ContainsOnlyAlphanumericChars`
- [x] `RandomString_CustomCharset_ContainsOnlyThoseChars`
- [x] `RandomInt_MaxExclusive_NeverEqualsMax`
- [x] `RandomLong_MaxExclusive_NeverEqualsMax`
- [x] `RandomItem_SingleItemList_ReturnsThatItem`
- [x] `RandomItem_EmptyList_Throws`

### `src/Storm.Api/Extensions/SemaphoreExtensions.cs`

- [x] `Lock_AcquiresSemaphore`
- [x] `Lock_DisposeReleasesSemaphoreOnce`
- [x] `Lock_DoubleDispose_ReleasesOnlyOnce`
- [x] `Lock_SerializesConcurrentAccess`

### `src/Storm.Api/Extensions/ServicesExtensions.cs`

- [x] `Now_DelegatesToTimeProvider` *(per CLAUDE.md — no DateTime.Now)*
- [x] `Create_UsesActivatorUtilities`
- [x] `ExecuteWithScope_Async_CreatesAndDisposesScope`
- [x] `ExecuteWithScope_Sync_CreatesAndDisposesScope`
- [x] `ExecuteWithScope_WithResult_ReturnsValue`
- [x] `ExecuteWithScope_ExceptionPropagates`
- [x] `ExecuteAction_ResolvesActionAndExecutes`

### `src/Storm.Api/Extensions/StringExtensions.cs`

- [x] `IsNullOrEmpty_Null_ReturnsTrue`
- [x] `IsNullOrEmpty_Empty_ReturnsTrue`
- [x] `IsNullOrEmpty_Whitespace_ReturnsFalse`
- [x] `IsNullOrWhiteSpace_Whitespace_ReturnsTrue`
- [x] `IsNotNullOrEmpty_NonEmpty_ReturnsTrue`
- [x] `IsNotNullOrWhiteSpace_NonWhitespace_ReturnsTrue`
- [x] `ValueIfNull_Null_ReturnsFallback`
- [x] `ValueIfNull_NonNull_ReturnsOriginal`
- [x] `ValueIfNullOrEmpty_Empty_ReturnsFallback`
- [x] `ValueIfNullOrWhiteSpace_Whitespace_ReturnsFallback`
- [x] `NullIfEmpty_Empty_ReturnsNull`
- [x] `NullIfEmpty_NonEmpty_ReturnsOriginal`
- [x] `NullIfWhiteSpace_Whitespace_ReturnsNull`
- [x] `OrEmpty_Null_ReturnsEmpty`
- [x] `OrEmpty_NonNull_ReturnsOriginal`
- [x] `AsInt_ValidInteger_ReturnsParsedInt`
- [x] `AsInt_Invalid_ReturnsZero`
- [x] `AsInt_Null_ReturnsZero`
- [x] `AsInt_Empty_ReturnsZero`

### `src/Storm.Api/Extensions/TasksExtensions.cs`

- [x] `AsTask_ReturnsCompletedTaskWithValue`
- [x] `AsTaskNullable_Struct_WrapsValue`
- [x] `AsTaskNullable_Null_ReturnsTaskWithNull`
- [x] `WaitForCancellation_AlreadyCancelled_CompletesImmediately`
- [x] `WaitForCancellation_CompletesWhenTokenCancelled`

---

## 6. Queues

### `src/Storm.Api/Queues/ItemQueue.cs`

- [x] `Queue_SingleItem_CanBeDequeued`
- [x] `Queue_MultipleItems_DequeuedInFifoOrder`
- [x] `Dequeue_EmptyQueue_BlocksUntilItemAvailable`
- [x] `Dequeue_CancellationTokenCancelled_ThrowsOperationCanceled`
- [x] `DequeueAll_IteratesAllQueuedItems`
- [x] `ConcurrentQueueAndDequeue_IsThreadSafe`

### `src/Storm.Api/Queues/BufferedItemQueue.cs`

- [x] `Dequeue_ReturnsUpToBufferSizeItems`
- [x] `Dequeue_ExactlyBufferSizeWhenEnoughAvailable`
- [x] `Dequeue_BlocksUntilAtLeastOneItem`
- [x] `Dequeue_CancellationReturnsPartialBuffer`
- [x] `Dequeue_ConcurrentQueueAndDequeue_IsThreadSafe`

### `src/Storm.Api/Queues/ThrottledBufferedItemQueue.cs`

- [x] `Dequeue_FirstItem_WaitsIndefinitely`
- [x] `Dequeue_SubsequentItems_ConstrainedByThrottlingTime`
- [x] `Dequeue_TimeoutAfterFirstItem_ReturnsPartialBuffer`
- [x] `Dequeue_AllArriveBeforeTimeout_ReturnsFullBuffer`
- [x] `Dequeue_CancelledBeforeFirstItem_ReturnsEmptyArray`
- [x] `Dequeue_ExceptionDuringRead_SwallowedCurrently` *(pins existing behavior — see Known issues)*

---

## 7. Workers & Retry Strategies

### `src/Storm.Api/Workers/BackgroundWorker.cs`

- [x] `Start_BeginsRun`
- [x] `Start_AlreadyRunning_IsNoOp`
- [x] `Stop_CancelsAndClearsTask`
- [x] `Stop_NotRunning_IsNoOp`
- [x] `Run_WrapsUserFunctionWithLogging`
- [x] `Run_ExceptionInUserFunction_LoggedAndSuppressed`
- [x] `ConcurrentStartStop_IsThreadSafe`

### `src/Storm.Api/Workers/HostedServices/BaseHostedService.cs`

- [x] `Resolve_ServiceRegistered_ReturnsInstance`
- [x] `Resolve_ServiceMissing_ThrowsInvalidOperation`

### `src/Storm.Api/Workers/HostedServices/BasePeriodicRunHostedService.cs`

- [x] `ExecuteAsync_CallsRunRepeatedly_RespectingInterval`
- [x] `ExecuteAsync_CreatesAndDisposesScopePerIteration`
- [x] `ExecuteAsync_CancellationStopsLoop`
- [x] `ExecuteAsync_ExceptionInRun_LoggedAndLoopContinues`

### `src/Storm.Api/Workers/HostedServices/BaseTimeRunHostedService.cs`

- [x] `AwaitNextRun_SingleTimeInFuture_ReturnsToday`
- [x] `AwaitNextRun_SingleTimeInPast_ReturnsTomorrow`
- [x] `AwaitNextRun_MultipleTimes_ReturnsEarliestUpcoming`
- [x] `AwaitNextRun_MidnightBoundary_Handled`
- [x] `AwaitNextRun_CancellationDuringWait_Throws`

### `src/Storm.Api/Workers/Strategies/DelayRetryStrategy.cs`

- [x] `Wait_DelaysForConfiguredMilliseconds`
- [x] `DiscardAfterFailedAttempts_SetWhenCountProvided`
- [x] `Reset_IsNoOp_ConstantDelay`

### `src/Storm.Api/Workers/Strategies/ExponentialBackOffStrategy.cs`

- [x] `Wait_Iteration1_DelayEqualsBase`
- [x] `Wait_Iteration2_DelayEqualsThreeTimesBase`
- [x] `Wait_IterationCapsAtMaxIteration`
- [x] `Reset_ResetsIterationCounter`
- [x] `DiscardAfterFailedAttempts_SetWhenCountProvided`

### `src/Storm.Api/Workers/Queues/AbstractBackgroundQueueWorker.cs`

- [x] `Queue_FirstItem_StartsWorker`
- [x] `ProcessItemsAsync_Success_ResetsRetryStrategy`
- [x] `ProcessItemsAsync_TransientFailure_WaitsAndRetries`
- [x] `ProcessItemsAsync_MaxAttemptsReached_DiscardsItem`
- [x] `ProcessItemsAsync_OnItemsSuccess_CalledWithProcessedItems`
- [x] `ProcessItemsAsync_OnItemsError_CalledWithFailure`
- [x] `ProcessItemsAsync_Cancellation_StopsProcessing`

---

## 8. Security / Auth (pure logic)

### `src/Storm.Api/Authentications/Pbkdf2Passwords.cs` — **HIGH PRIORITY**

- [x] `HashPassword_OutputHasFiveDotSeparatedParts`
- [x] `HashPassword_PartsAreBase64WhereExpected`
- [x] `HashPassword_Sha256_DefaultAlgorithm`
- [x] `HashPassword_Sha512_SelectableAlgorithm`
- [x] `HashPassword_SameInputDifferentCalls_ProduceDifferentHashes` *(due to random salt)*
- [x] `IsValid_CorrectPassword_ReturnsTrue`
- [x] `IsValid_WrongPassword_ReturnsFalse`
- [x] `IsValid_MalformedHash_ReturnsFalse`
- [x] `IsValid_InvalidBase64Parts_ReturnsFalse`
- [x] `IsValid_UnknownAlgorithm_ReturnsFalse`
- [x] `IsValid_UsesConstantTimeComparison` *(smoke test — exercise both branches)*
- [x] `CreateSalt_ProducesRequestedByteCount`
- [x] `CreateSalt_IsNonDeterministic`

### `src/Storm.Api/Authentications/Jwts/JwtTokenService.cs` — **HIGH PRIORITY**

- [x] `GenerateToken_IncludesSubjectClaim`
- [x] `GenerateToken_IncludesAudienceAndIssuer`
- [x] `GenerateToken_IncludesExpirationBasedOnTimeProvider`
- [x] `GenerateToken_IncludesAdditionalClaims_WhenProvided`
- [x] `TryGetIdWithoutValidation_ReturnsClaimIdEvenIfTampered`
- [x] `TryGetId_ValidToken_ReturnsId`
- [x] `TryGetId_TamperedSignature_ReturnsFalse`
- [ ] `TryGetId_ExpiredToken_ReturnsFalse`
- [x] `TryGetId_WrongAudience_ReturnsFalse`
- [x] `TryGetId_WrongIssuer_ReturnsFalse`
- [x] `TryGetId_Malformed_ReturnsFalse`
- [x] `IsValid_WrapsTryGetId`
- [x] `Guid_Id_RoundtripsCorrectly`

### `src/Storm.Api/Authentications/Commons/BaseTokenAuthenticator.cs`

- [x] `Authenticate_HeaderPresent_ExtractsToken`
- [x] `Authenticate_QueryPresent_ExtractsToken`
- [x] `Authenticate_BearerPrefixStripped`
- [x] `Authenticate_NoToken_ReturnsNull`
- [x] `Authenticate_CallsAbstractAuthenticateOnlyWithTokenString`

### `src/Storm.Api/Authentications/Refresh/BaseRefreshAction.cs`

- [ ] `Action_ReadsTokenFromTransport`
- [ ] `Action_TransportValidationFails_ThrowsUnauthorized`
- [ ] `Action_InvalidJwt_ThrowsUnauthorized`
- [ ] `Action_MissingJti_ThrowsUnauthorized`
- [ ] `Action_JtiNotInStorage_ThrowsUnauthorized`
- [ ] `Action_AccountValidationFails_ThrowsUnauthorized`
- [ ] `Action_HappyPath_GeneratesNewAccessAndRefreshTokens`
- [ ] `Action_HappyPath_RotatesJtiAtomically`

### `src/Storm.Api/Authentications/Refresh/Transport/CookieRefreshTokenTransport.cs`

- [x] `ReadToken_CookieMissing_ReturnsNull`
- [x] `ReadToken_CookiePresent_ReturnsValue`
- [x] `ValidateRequest_CsrfHeaderMissing_Throws`
- [x] `ValidateRequest_CsrfHeaderMatches_Passes`
- [x] `WriteToken_SetsCookieWithExpectedAttributes`

### `src/Storm.Api/Authentications/Refresh/Transport/JsonRefreshTokenTransport.cs`

- [x] `ReadToken_JsonBodyContainsToken_ReturnsValue`
- [x] `ReadToken_JsonBodyEmpty_ReturnsNull`
- [x] `WriteToken_ReturnsJsonResponseWithToken`

---

## 9. Databases (pure logic only)

### `src/Storm.Api/Databases/SequentialGuid.cs`

- [x] `NewGuid_ConsecutiveCalls_AreChronologicallyOrdered`
- [x] `NewGuid_PostgresLikeProvider_ReturnsUuidV7Format`
- [x] `CreateSqlServerSequentialGuid_TimestampInBytes10To15`
- [x] `CreateSqlServerSequentialGuid_SameTickSequence_SortsCorrectly`
- [x] `NewGuid_WithMockedTimeProvider_IsDeterministic`

### `src/Storm.Api/Databases/Converters/SqlServerDateOnlyConverter.cs`

- [x] `ToDbValue_ReturnsIsoFormattedString`
- [x] `FromDbValue_StringInput_ParsesToDateOnly`
- [x] `FromDbValue_DateTimeInput_ConvertsToDateOnly`
- [x] `ToQuotedString_WrapsValueInSingleQuotes`
- [x] `RoundTrip_PreservesDateExactly`
- [x] `FromDbValue_InvalidFormat_Throws`

### `src/Storm.Api/Databases/Converters/SqlServerTimeOnlyConverter.cs`

- [x] `ToDbValue_ReturnsHhMmSsFormat`
- [x] `FromDbValue_StringInput_ParsesToTimeOnly`
- [x] `FromDbValue_TimeSpanInput_ConvertsToTimeOnly`
- [x] `ToQuotedString_WrapsValueInSingleQuotes`
- [x] `RoundTrip_PreservesTimeExactly`
- [x] `BoundaryValues_Midnight_And_EndOfDay_Handled`

### `src/Storm.Api/Databases/Converters/DapperMappers/DateOnlyTypeHandler.cs`

- [x] `SetValue_DateOnly_AssignsDateTimeParameter`
- [x] `Parse_DateTimeInput_ReturnsDateOnly`
- [x] `Parse_NullInput_ReturnsDefault`

### `src/Storm.Api/Databases/Converters/DapperMappers/TimeOnlyTypeHandler.cs`

- [x] `SetValue_TimeOnly_AssignsTimeSpanParameter`
- [x] `Parse_TimeSpanInput_ReturnsTimeOnly`
- [x] `Parse_NullInput_ReturnsDefault`

### `src/Storm.Api/Databases/Connections/DatabaseTransaction.cs`

- [x] `Commit_CallsUnderlyingCommit_MarksFinalized`
- [x] `Rollback_CallsUnderlyingRollback_MarksFinalized`
- [x] `Dispose_WhenNotFinalized_EndsTransaction`
- [x] `Dispose_WhenFinalized_IsNoOp`
- [x] `Dispose_CalledTwice_IsIdempotent`
- [ ] `EndTransaction_ZombieTransaction_SwallowsInvalidOperation`

### `src/Storm.Api/Databases/Services/DatabaseService.cs`

- [x] `GetWriteConnection_FirstCall_CreatesConnection`
- [x] `GetWriteConnection_SecondCall_ReusesConnection`
- [x] `GetReadConnection_NoReadReplica_RoutesToWrite`
- [x] `GetReadConnection_WithReadReplica_UsesReadConnection`
- [x] `CreateTransaction_NotInTransaction_ReturnsRealTransaction`
- [x] `CreateTransaction_NestedCall_ReturnsDummyTransaction`
- [x] `InTransaction_Success_CommitsAutomatically`
- [x] `InTransaction_Throws_RollsBackAndRethrows`

### `src/Storm.Api/Databases/Internals/SqlFieldsOrdering.cs`

- [x] `ReorderField_IdFieldMovedToFront`
- [x] `ReorderField_SoftDeleteFieldsMovedToEnd`
- [x] `ReorderField_BusinessFieldsPreserveOriginalOrder`
- [x] `ReorderField_NoIdField_HandledGracefully`

### `src/Storm.Api/Databases/Migrations/MigrationEngine.cs`

- [ ] `Run_ExecutesPendingMigrationsInOrder`
- [ ] `Run_SkipsAlreadyAppliedMigrations`
- [ ] `Run_WrapsEachMigrationInTransaction`
- [ ] `Run_MigrationFails_RollsBackTransaction`
- [ ] `Run_RecordsAppliedMigrationInHistoryTable`
- [ ] `Run_OldAndNewMigrationFormats_BothHandled`

---

## 10. Logs

### `src/Storm.Api/Logs/LogService.cs`

- [x] `Log_BelowMinimumLevel_Discarded`
- [x] `Log_AtOrAboveMinimumLevel_Forwarded`
- [x] `Log_AppliesAllAppenders`
- [x] `WithAppender_DuplicateWhenMultipleAllowedFalse_DeduplicatesByType`
- [x] `WithAppender_DuplicateWhenMultipleAllowedTrue_AddsBoth`
- [x] `WithAppenders_BatchRegistration_AddsAll`
- [x] `Log_NullContent_HandledWithoutThrowing`

---

## 11. Vaults

### `src/Storm.Api/Vaults/VaultConfigurationProvider.cs`

- [x] `AddData_FlatString_AddsSingleEntry`
- [x] `AddData_NestedJsonObject_FlattensToColonSeparatedKeys`
- [x] `AddData_DeeplyNestedObject_FullyFlattened`
- [x] `AddData_ArrayValues_IndicesUsedInKey`

### `src/Storm.Api/Vaults/VaultExtensions.cs`

- [x] `LoadVaultConfiguration_SectionPresent_PopulatesFields`
- [x] `LoadVaultConfiguration_KeysAsCommaSeparatedString_ParsedToArray`
- [x] `LoadVaultConfiguration_KeysAsSemicolonSeparatedString_ParsedToArray`
- [x] `LoadVaultConfiguration_KeysAsArray_ParsedDirectly`
- [x] `AddVault_SectionMissing_NoProviderAdded`
- [x] `AddVault_ConfigurationObject_RegistersProvider`

---

## 12. Helpers

### `src/Storm.Api/Helpers/TemporaryEmailDomains.cs`

- [x] `IsTemporaryEmailDomain_KnownDisposableDomain_ReturnsTrue`
- [x] `IsTemporaryEmailDomain_LegitimateDomain_ReturnsFalse`
- [x] `IsTemporaryEmailDomain_MixedCaseDomain_MatchesCaseInsensitively`
- [x] `IsTemporaryEmailDomain_MalformedEmail_ReturnsFalse`
- [x] `IsTemporaryEmailDomain_NullOrEmpty_ReturnsFalse`

---

## 13. Storm.Api.Tools

### `src/Storm.Api.Tools/Program.cs`

- [x] `Args_NoCommand_PrintsHelp`
- [x] `Args_UnknownCommand_ReturnsNonZeroExit`
- [x] `GenerateClaudeSkills_DefaultOutputDirectory_WritesFiles`
- [x] `GenerateClaudeSkills_CustomOutput_WritesToProvidedPath`
- [x] `GenerateClaudeSkills_ExistingFile_NoForce_Skipped`
- [x] `GenerateClaudeSkills_ExistingFile_WithForce_Overwritten`
- [x] `GenerateClaudeSkills_ExtractsEmbeddedResources`

---

## 14. Out of scope (integration, not unit)

The following need a real external dependency (DB, HTTP, Redis, or full ASP.NET Core pipeline). They belong in a separate integration test project, not in this backlog.

- Repository base classes (`BaseGuidRepository`, `BaseLongRepository`, …) — exercise OrmLite against a real DB.
- Log sinks: `ConsoleSink`, `SerilogSink`, `ElasticsearchSink`.
- `RedisService` — requires StackExchange.Redis connectivity.
- Resend email provider — requires HTTP to `api.resend.com`.
- `BaseStartup`, `DefaultLauncher<TStartup>`, `ConfigurationLoaderHelper` — full ASP.NET Core pipeline.
- Migration extension helpers (`ColumnExists`, `DropColumnIfExists`, `AddColumnIfNotExists`, SQL-Server-specific variants) — run real DDL.
- `DatabaseExtensions.AsSelectAsync` et al. — OrmLite pass-throughs.

---

## Known issues surfaced during exploration

Capture these as failing tests that pin the current (buggy) behavior until fixed:

- **`ThrottledBufferedItemQueue.Dequeue`** — a broad `catch` around the read loop swallows all exceptions silently. A test should exercise this path so any future change is intentional.
